using Slotwise.Application.Sessions.Commands.BookSession;
using Slotwise.Application.Sessions.Commands.CancelBooking;
using Slotwise.Application.Sessions.Commands.CreateSession;
using Slotwise.Application.Sessions.Queries.GetSession;
using Slotwise.Application.Sessions.Queries.ListSessions;

namespace Slotwise.Application.Tests;

public class CommandAndQueryRecordTests
{
    [Fact]
    public void CreateSessionCommand_HasValueEquality()
    {
        var start = new DateTime(2030, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var command = new CreateSessionCommand("Yoga", start, start.AddHours(1), 10);
        var copy = command with { };

        Assert.Equal(command, copy);
        Assert.Equal(command.GetHashCode(), copy.GetHashCode());
        Assert.Contains("Yoga", command.ToString());
        Assert.NotEqual(command, new CreateSessionCommand("Pilates", start, start.AddHours(1), 10));
    }

    [Fact]
    public void BookSessionCommand_HasValueEquality()
    {
        var id = Guid.NewGuid();

        Assert.Equal(new BookSessionCommand(id, "a@example.com"), new BookSessionCommand(id, "a@example.com"));
    }

    [Fact]
    public void CancelBookingCommand_HasValueEquality()
    {
        var sessionId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();

        Assert.Equal(new CancelBookingCommand(sessionId, bookingId), new CancelBookingCommand(sessionId, bookingId));
    }

    [Fact]
    public void GetSessionQuery_HasValueEquality()
    {
        var id = Guid.NewGuid();

        Assert.Equal(new GetSessionQuery(id), new GetSessionQuery(id));
    }

    [Fact]
    public void ListSessionsQuery_HasValueEquality()
    {
        var query = new ListSessionsQuery();

        Assert.Equal(query, query with { });
        Assert.Equal(query.GetHashCode(), (query with { }).GetHashCode());
        Assert.Contains(nameof(ListSessionsQuery), query.ToString());
    }
}
