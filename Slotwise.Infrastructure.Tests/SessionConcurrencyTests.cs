using Microsoft.EntityFrameworkCore;
using Slotwise.Application.Sessions.Exceptions;
using Slotwise.Domain.Sessions.Entities;
using Slotwise.Domain.Sessions.ValueObjects;
using Slotwise.Infrastructure.Persistence.Repositories;

namespace Slotwise.Infrastructure.Tests;

[Collection(SqlServerCollection.Name)]
public class SessionConcurrencyTests(SqlServerFixture db)
{
    private static Session NewSession(int capacity)
    {
        var start = DateTime.UtcNow.AddDays(1);
        return new Session("Yoga", new TimeSlot(start, start.AddHours(1)), new SeatCount(capacity));
    }

    private async Task SaveNewAsync(Session session)
    {
        var context = db.CreateContext();

        try
        {
            var repository = new SessionRepository(context);
            repository.Add(session);
            await repository.SaveChangesAsync();
        }
        finally
        {
            if (context is not null)
            {
                await context.DisposeAsync();
            }
        }
    }

    private async Task<Session?> LoadAsync(Guid id)
    {
        var context = db.CreateContext();

        try
        {
            return await new SessionRepository(context).GetByIdAsync(id);
        }
        finally
        {
            if (context is not null)
            {
                await context.DisposeAsync();
            }
        }
    }

    private async Task BookAsync(Guid sessionId, string email)
    {
        var context = db.CreateContext();

        try
        {
            var repository = new SessionRepository(context);
            var loaded = await repository.GetByIdAsync(sessionId);
            loaded!.Book(new EmailAddress(email));
            await repository.SaveChangesAsync();
        }
        finally
        {
            if (context is not null)
            {
                await context.DisposeAsync();
            }
        }
    }

    private async Task<int> ReadVersionAsync(Guid sessionId)
    {
        await using var context = db.CreateContext();

        return await context.Sessions
            .AsNoTracking()
            .Where(s => s.Id == sessionId)
            .Select(s => EF.Property<int>(s, "Version"))
            .SingleAsync();
    }

    [Fact]
    public async Task TwoRequestsBookingTheLastSeat_OnlyTheFirstIsSaved()
    {
        var session = NewSession(capacity: 1);
        await SaveNewAsync(session);

        var contextA = db.CreateContext();
        var contextB = db.CreateContext();

        try
        {
            var repositoryA = new SessionRepository(contextA);
            var repositoryB = new SessionRepository(contextB);

            var sessionA = await repositoryA.GetByIdAsync(session.Id);
            var sessionB = await repositoryB.GetByIdAsync(session.Id);

            sessionA!.Book(new EmailAddress("a@example.com"));
            sessionB!.Book(new EmailAddress("b@example.com"));

            await repositoryA.SaveChangesAsync();

            await Assert.ThrowsAsync<SessionConcurrencyException>(() => repositoryB.SaveChangesAsync());
        }
        finally
        {
            if (contextA is not null)
            {
                await contextA.DisposeAsync();
            }

            if (contextB is not null)
            {
                await contextB.DisposeAsync();
            }
        }

        var reloaded = await LoadAsync(session.Id);
        var booking = Assert.Single(reloaded!.Bookings);
        Assert.Equal("a@example.com", booking.EmailAddress.Value);
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
    }

    [Fact]
    public async Task BookingsInSeparateRequests_BothSucceed()
    {
        var session = NewSession(capacity: 5);
        await SaveNewAsync(session);

        await BookAsync(session.Id, "a@example.com");
        await BookAsync(session.Id, "b@example.com");

        var reloaded = await LoadAsync(session.Id);
        Assert.Equal(2, reloaded!.Bookings.Count);
    }

    [Fact]
    public async Task EachBookingChange_IncrementsTheSessionVersionByOne()
    {
        var session = NewSession(capacity: 5);
        await SaveNewAsync(session);
        Assert.Equal(0, await ReadVersionAsync(session.Id));

        await BookAsync(session.Id, "a@example.com");
        Assert.Equal(1, await ReadVersionAsync(session.Id));

        var context = db.CreateContext();

        try
        {
            var repository = new SessionRepository(context);
            var loaded = await repository.GetByIdAsync(session.Id);
            loaded!.CancelBooking(loaded.Bookings.Single().Id);
            await repository.SaveChangesAsync();
        }
        finally
        {
            if (context is not null)
            {
                await context.DisposeAsync();
            }
        }

        Assert.Equal(2, await ReadVersionAsync(session.Id));
    }
}
