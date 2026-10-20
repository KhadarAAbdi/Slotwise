using Slotwise.Domain.Sessions.Entities;
using Slotwise.Domain.Sessions.Events;
using Slotwise.Domain.Sessions.ValueObjects;

namespace Slotwise.Domain.Tests;

public class DomainEventTests
{
    private static Session CreateSession(int capacity = 1)
    {
        var start = DateTime.UtcNow.AddDays(1);
        return new Session("Yoga", new TimeSlot(start, start.AddHours(1)), new SeatCount(capacity));
    }

    private static void AssertRecent(DateTime occurredOn)
    {
        Assert.InRange(occurredOn, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
    }

    [Fact]
    public void SessionCreated_IsRaisedOnCreation_AndCarriesSession()
    {
        var session = CreateSession();

        var evt = Assert.Single(session.DomainEvents.OfType<SessionCreated>());

        Assert.Equal(session.Id, evt.SessionId);
        AssertRecent(evt.OccurredOn);
    }

    [Fact]
    public void BookingConfirmed_CarriesSessionBookingAndEmail()
    {
        var session = CreateSession();
        var booking = session.Book(new EmailAddress("a@example.com"));

        var evt = Assert.Single(session.DomainEvents.OfType<BookingConfirmed>());

        Assert.Equal(session.Id, evt.SessionId);
        Assert.Equal(booking.Id, evt.BookingId);
        Assert.Equal("a@example.com", evt.AttendeeEmail);
        AssertRecent(evt.OccurredOn);
    }

    [Fact]
    public void BookingCancelled_CarriesSessionBookingAndEmail()
    {
        var session = CreateSession();
        var booking = session.Book(new EmailAddress("a@example.com"));
        session.CancelBooking(booking.Id);

        var evt = Assert.Single(session.DomainEvents.OfType<BookingCancelled>());

        Assert.Equal(session.Id, evt.SessionId);
        Assert.Equal(booking.Id, evt.BookingId);
        Assert.Equal("a@example.com", evt.AttendeeEmail);
        AssertRecent(evt.OccurredOn);
    }

    [Fact]
    public void SpotOpened_CarriesSession()
    {
        var session = CreateSession();
        var booking = session.Book(new EmailAddress("a@example.com"));
        session.CancelBooking(booking.Id);

        var evt = Assert.Single(session.DomainEvents.OfType<SpotOpened>());

        Assert.Equal(session.Id, evt.SessionId);
        AssertRecent(evt.OccurredOn);
    }

    [Fact]
    public void WaitlistPromoted_CarriesSessionBookingAndEmail()
    {
        var session = CreateSession();
        var confirmed = session.Book(new EmailAddress("a@example.com"));
        var waitlisted = session.Book(new EmailAddress("b@example.com"));
        session.CancelBooking(confirmed.Id);
        session.PromoteFromWaitlist(waitlisted.Id);

        var evt = Assert.Single(session.DomainEvents.OfType<WaitlistPromoted>());

        Assert.Equal(session.Id, evt.SessionId);
        Assert.Equal(waitlisted.Id, evt.BookingId);
        Assert.Equal("b@example.com", evt.AttendeeEmail);
        AssertRecent(evt.OccurredOn);
    }

    [Fact]
    public void Events_AreRecordsWithValueSemantics()
    {
        var sessionId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();

        var created = new SessionCreated(sessionId);
        var spotOpened = new SpotOpened(sessionId);
        var confirmed = new BookingConfirmed(sessionId, bookingId, "a@example.com");
        var cancelled = new BookingCancelled(sessionId, bookingId, "a@example.com");
        var promoted = new WaitlistPromoted(sessionId, bookingId, "a@example.com");

        AssertValueSemantics(created, created with { }, sessionId);
        AssertValueSemantics(spotOpened, spotOpened with { }, sessionId);
        AssertValueSemantics(confirmed, confirmed with { }, bookingId);
        AssertValueSemantics(cancelled, cancelled with { }, bookingId);
        AssertValueSemantics(promoted, promoted with { }, bookingId);
    }

    [Fact]
    public void BookingWaitlisted_IsRaisedWhenTheSessionIsFull_AndCarriesSessionBookingAndEmail()
    {
        Session session = CreateSession(1);
        session.Book(new EmailAddress("a@example.com"));
        Booking booking = session.Book(new EmailAddress("b@example.com"));

        BookingWaitlisted evt = Assert.Single(session.DomainEvents.OfType<BookingWaitlisted>());


        Assert.Equal(session.Id, evt.SessionId);
        Assert.Equal(booking.Id, evt.BookingId);
        Assert.Equal(booking.EmailAddress.ToString(), evt.AttendeeEmail);
        AssertRecent(evt.OccurredOn);

    }

    private static void AssertValueSemantics(object evt, object copy, Guid idInToString)
    {
        Assert.Equal(evt, copy);
        Assert.Equal(evt.GetHashCode(), copy.GetHashCode());
        Assert.Contains(idInToString.ToString(), evt.ToString());
    }
}
