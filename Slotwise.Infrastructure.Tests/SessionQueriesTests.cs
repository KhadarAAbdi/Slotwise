using Slotwise.Application.Sessions.DTOs;
using Slotwise.Domain.Sessions.Entities;
using Slotwise.Domain.Sessions.ValueObjects;
using Slotwise.Infrastructure.Persistence.Queries;
using Slotwise.Infrastructure.Persistence.Repositories;

namespace Slotwise.Infrastructure.Tests;

[Collection(SqlServerCollection.Name)]
public class SessionQueriesTests(SqlServerFixture db)
{
    private static Session NewSession(string title, int capacity)
    {
        var start = DateTime.UtcNow.AddDays(1);
        return new Session(title, new TimeSlot(start, start.AddHours(1)), new SeatCount(capacity));
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

    private async Task<SessionDTO?> GetByIdAsync(Guid id)
    {
        var context = db.CreateContext();

        try
        {
            return await new SessionQueries(context).GetByIdAsync(id);
        }
        finally
        {
            if (context is not null)
            {
                await context.DisposeAsync();
            }
        }
    }

    private async Task<IReadOnlyList<SessionDTO>> ListAsync()
    {
        var context = db.CreateContext();

        try
        {
            return await new SessionQueries(context).ListAsync();
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
    public async Task GetByIdAsync_ReturnsDetailsCountsAndEveryBooking()
    {
        var session = NewSession("Pilates", capacity: 2);
        session.Book(new EmailAddress("a@example.com"));
        var cancelled = session.Book(new EmailAddress("b@example.com"));
        session.Book(new EmailAddress("c@example.com"));
        session.Book(new EmailAddress("d@example.com"));
        session.CancelBooking(cancelled.Id);
        await SaveNewAsync(session);

        var dto = await GetByIdAsync(session.Id);

        Assert.NotNull(dto);
        Assert.Equal(session.Id, dto.Id);
        Assert.Equal("Pilates", dto.Title);
        Assert.Equal(session.TimeSlot.Start, dto.Start);
        Assert.Equal(session.TimeSlot.End, dto.End);
        Assert.Equal(2, dto.Capacity);
        Assert.Equal(1, dto.ConfirmedCount);
        Assert.Equal(2, dto.WaitlistCount);

        var statuses = dto.Bookings.ToDictionary(b => b.Email, b => b.Status);
        Assert.Equal(4, statuses.Count);
        Assert.Equal("Confirmed", statuses["a@example.com"]);
        Assert.Equal("Cancelled", statuses["b@example.com"]);
        Assert.Equal("Waitlisted", statuses["c@example.com"]);
        Assert.Equal("Waitlisted", statuses["d@example.com"]);
    }

    [Fact]
    public async Task GetByIdAsync_SessionWithoutBookings_ReturnsZeroCountsAndNoBookings()
    {
        var session = NewSession("Empty", capacity: 3);
        await SaveNewAsync(session);

        var dto = await GetByIdAsync(session.Id);

        Assert.NotNull(dto);
        Assert.Equal(0, dto.ConfirmedCount);
        Assert.Equal(0, dto.WaitlistCount);
        Assert.Empty(dto.Bookings);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        Assert.Null(await GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ListAsync_ReturnsEverySavedSessionWithItsOwnCounts()
    {
        var first = NewSession("List one", capacity: 1);
        first.Book(new EmailAddress("a@example.com"));
        var second = NewSession("List two", capacity: 4);
        await SaveNewAsync(first);
        await SaveNewAsync(second);

        var all = await ListAsync();

        var one = Assert.Single(all, s => s.Id == first.Id);
        Assert.Equal("List one", one.Title);
        Assert.Equal(1, one.ConfirmedCount);
        Assert.Single(one.Bookings);

        var two = Assert.Single(all, s => s.Id == second.Id);
        Assert.Equal("List two", two.Title);
        Assert.Equal(4, two.Capacity);
        Assert.Equal(0, two.ConfirmedCount);
        Assert.Empty(two.Bookings);
    }
}
