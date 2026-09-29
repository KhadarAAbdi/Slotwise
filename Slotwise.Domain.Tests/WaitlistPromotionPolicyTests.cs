using Slotwise.Domain.Sessions.Entities;
using Slotwise.Domain.Sessions.Services;
using Slotwise.Domain.Sessions.ValueObjects;

namespace Slotwise.Domain.Tests;

public class WaitlistPromotionPolicyTests
{
    private static Session CreateSession(int capacity)
    {
        var start = DateTime.UtcNow.AddDays(1);
        return new Session("Yoga", new TimeSlot(start, start.AddHours(1)), new SeatCount(capacity));
    }

    [Fact]
    public void TryPromoteNext_PromotesLongestWaitingBooking()
    {
        var session = CreateSession(capacity: 1);
        var confirmed = session.Book(new EmailAddress("a@example.com"));
        var firstInLine = session.Book(new EmailAddress("b@example.com"));
        var secondInLine = session.Book(new EmailAddress("c@example.com"));
        session.CancelBooking(confirmed.Id);

        var promoted = new WaitlistPromotionPolicy().TryPromoteNext(session);

        Assert.True(promoted);
        Assert.Equal(BookingStatus.Confirmed, firstInLine.Status);
        Assert.Equal(BookingStatus.Waitlisted, secondInLine.Status);
    }

    [Fact]
    public void TryPromoteNext_EmptyWaitlist_ReturnsFalse()
    {
        var session = CreateSession(capacity: 2);
        session.Book(new EmailAddress("a@example.com"));

        var promoted = new WaitlistPromotionPolicy().TryPromoteNext(session);

        Assert.False(promoted);
    }
}
