using Microsoft.EntityFrameworkCore;
using Slotwise.Domain.Sessions.Entities;
using Slotwise.Domain.Sessions.ValueObjects;
using Slotwise.Infrastructure.Persistence.Repositories;

namespace Slotwise.Infrastructure.Tests;

[Collection(SqlServerCollection.Name)]
public class SessionPersistenceTests(SqlServerFixture db)
{
    private static Session NewSession(int capacity = 2)
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

    [Fact]
    public async Task Session_WithConfirmedAndWaitlistedBookings_SurvivesARoundTrip()
    {
        var session = NewSession(capacity: 1);
        var confirmed = session.Book(new EmailAddress("a@example.com"));
        var waitlisted = session.Book(new EmailAddress("b@example.com"));
        await SaveNewAsync(session);

        var loaded = await LoadAsync(session.Id);

        Assert.NotNull(loaded);
        Assert.Equal("Yoga", loaded.Title);
        Assert.Equal(session.TimeSlot.Start, loaded.TimeSlot.Start);
        Assert.Equal(session.TimeSlot.End, loaded.TimeSlot.End);
        Assert.Equal(1, loaded.SeatCount.Value);
        Assert.Equal(2, loaded.Bookings.Count);

        var loadedConfirmed = Assert.Single(loaded.Bookings, b => b.Id == confirmed.Id);
        Assert.Equal(BookingStatus.Confirmed, loadedConfirmed.Status);
        Assert.Equal("a@example.com", loadedConfirmed.EmailAddress.Value);
        Assert.Equal(session.Id, loadedConfirmed.SessionId);

        var loadedWaitlisted = Assert.Single(loaded.Bookings, b => b.Id == waitlisted.Id);
        Assert.Equal(BookingStatus.Waitlisted, loadedWaitlisted.Status);
        Assert.Equal("b@example.com", loadedWaitlisted.EmailAddress.Value);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        Assert.Null(await LoadAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task Book_OnALoadedSession_PersistsTheNewBooking()
    {
        var session = NewSession(capacity: 5);
        session.Book(new EmailAddress("a@example.com"));
        await SaveNewAsync(session);

        var context = db.CreateContext();

        try
        {
            var repository = new SessionRepository(context);
            var loaded = await repository.GetByIdAsync(session.Id);
            loaded!.Book(new EmailAddress("b@example.com"));
            await repository.SaveChangesAsync();
        }
        finally
        {
            if (context is not null)
            {
                await context.DisposeAsync();
            }
        }

        var reloaded = await LoadAsync(session.Id);
        Assert.Equal(2, reloaded!.Bookings.Count);
        Assert.Contains(reloaded.Bookings, b => b.EmailAddress.Value == "b@example.com");
    }

    [Fact]
    public async Task CancelBooking_OnALoadedSession_PersistsTheStatusChange()
    {
        var session = NewSession();
        var booking = session.Book(new EmailAddress("a@example.com"));
        await SaveNewAsync(session);

        var context = db.CreateContext();

        try
        {
            var repository = new SessionRepository(context);
            var loaded = await repository.GetByIdAsync(session.Id);
            loaded!.CancelBooking(booking.Id);
            await repository.SaveChangesAsync();
        }
        finally
        {
            if (context is not null)
            {
                await context.DisposeAsync();
            }
        }

        var reloaded = await LoadAsync(session.Id);
        Assert.Equal(BookingStatus.Cancelled, Assert.Single(reloaded!.Bookings).Status);
    }

    [Fact]
    public async Task BookingStatus_IsStoredAsItsName()
    {
        var session = NewSession(capacity: 1);
        session.Book(new EmailAddress("a@example.com"));
        session.Book(new EmailAddress("b@example.com"));
        await SaveNewAsync(session);

        List<string> stored;
        var context = db.CreateContext();

        try
        {
            var table = context.Model.FindEntityType(typeof(Booking))!.GetTableName();
#pragma warning disable EF1002 // the table name comes from the EF model, not from user input
            stored = await context.Database
                .SqlQueryRaw<string>($"SELECT Status AS [Value] FROM [{table}] WHERE SessionId = {{0}}", session.Id)
                .ToListAsync();
#pragma warning restore EF1002
        }
        finally
        {
            if (context is not null)
            {
                await context.DisposeAsync();
            }
        }

        Assert.Equal(new[] { "Confirmed", "Waitlisted" }, stored.OrderBy(s => s).ToArray());
    }
}
