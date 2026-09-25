using Slotwise.Domain.Sessions.Entities;
using Slotwise.Domain.Sessions.Events;
using Slotwise.Domain.Sessions.ValueObjects;

namespace Slotwise.Domain.Tests;

public class SessionTests
{
    private static Session CreateSession(int capacity = 2)
    {
        var start = DateTime.UtcNow.AddDays(1);
        return new Session("Yoga", new TimeSlot(start, start.AddHours(1)), new SeatCount(capacity));
    }

    [Fact]
    public void Book_SameEmailTwice_Throws()
    {
        var session = CreateSession();
        session.Book(new EmailAddress("sam@example.com"));

        Assert.Throws<InvalidOperationException>(() => session.Book(new EmailAddress("sam@example.com")));
    }

    [Fact]
    public void Book_SameEmailAfterCancelling_Succeeds()
    {
        var session = CreateSession();
        var first = session.Book(new EmailAddress("sam@example.com"));
        session.CancelBooking(first.Id);

        var second = session.Book(new EmailAddress("sam@example.com"));

        Assert.Equal(BookingStatus.Confirmed, second.Status);
    }

    [Fact]
    public void Book_UpToCapacity_ConfirmsAndNextIsWaitlisted()
    {
        var session = CreateSession(capacity: 2);

        var first = session.Book(new EmailAddress("a@example.com"));
        var second = session.Book(new EmailAddress("b@example.com"));
        var third = session.Book(new EmailAddress("c@example.com"));

        Assert.Equal(BookingStatus.Confirmed, first.Status);
        Assert.Equal(BookingStatus.Confirmed, second.Status);
        Assert.Equal(BookingStatus.Waitlisted, third.Status);
    }

    [Fact]
    public void Book_Confirmed_RaisesBookingConfirmed()
    {
        var session = CreateSession();

        session.Book(new EmailAddress("a@example.com"));

        Assert.Single(session.DomainEvents.OfType<BookingConfirmed>());
    }

    [Fact]
    public void Book_Waitlisted_RaisesNoBookingConfirmed()
    {
        var session = CreateSession(capacity: 1);
        session.Book(new EmailAddress("a@example.com"));
        session.ClearDomainEvents();

        session.Book(new EmailAddress("b@example.com"));

        Assert.Empty(session.DomainEvents.OfType<BookingConfirmed>());
    }

    [Fact]
    public void CancelBooking_ConfirmedBooking_RaisesCancelledAndSpotOpened()
    {
        var session = CreateSession();
        var booking = session.Book(new EmailAddress("a@example.com"));
        session.ClearDomainEvents();

        session.CancelBooking(booking.Id);

        Assert.Single(session.DomainEvents.OfType<BookingCancelled>());
        Assert.Single(session.DomainEvents.OfType<SpotOpened>());
    }

    [Fact]
    public void CancelBooking_WaitlistedBooking_DoesNotRaiseSpotOpened()
    {
        var session = CreateSession(capacity: 1);
        session.Book(new EmailAddress("a@example.com"));
        var waitlisted = session.Book(new EmailAddress("b@example.com"));
        session.ClearDomainEvents();

        session.CancelBooking(waitlisted.Id);

        Assert.Single(session.DomainEvents.OfType<BookingCancelled>());
        Assert.Empty(session.DomainEvents.OfType<SpotOpened>());
    }

    [Fact]
    public void CancelBooking_UnknownBooking_Throws()
    {
        var session = CreateSession();

        Assert.Throws<InvalidOperationException>(() => session.CancelBooking(Guid.NewGuid()));
    }

    [Fact]
    public void PromoteFromWaitlist_WhenSessionFull_Throws()
    {
        var session = CreateSession(capacity: 1);
        session.Book(new EmailAddress("a@example.com"));
        var waitlisted = session.Book(new EmailAddress("b@example.com"));

        Assert.Throws<InvalidOperationException>(() => session.PromoteFromWaitlist(waitlisted.Id));
    }

    [Fact]
    public void PromoteFromWaitlist_AfterSpotOpens_ConfirmsAndRaisesWaitlistPromoted()
    {
        var session = CreateSession(capacity: 1);
        var confirmed = session.Book(new EmailAddress("a@example.com"));
        var waitlisted = session.Book(new EmailAddress("b@example.com"));
        session.CancelBooking(confirmed.Id);

        session.PromoteFromWaitlist(waitlisted.Id);

        Assert.Equal(BookingStatus.Confirmed, waitlisted.Status);
        Assert.Single(session.DomainEvents.OfType<WaitlistPromoted>());
    }

    [Fact]
    public void Book_SameEmailDifferentCase_Throws()
    {
        var session = CreateSession();
        session.Book(new EmailAddress("sam@example.com"));

        Assert.Throws<InvalidOperationException>(() => session.Book(new EmailAddress("SAM@Example.com")));
    }
}
