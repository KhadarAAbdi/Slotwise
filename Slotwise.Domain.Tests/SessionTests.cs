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
    public void Constructor_ValidArguments_SetsProperties()
    {
        var start = DateTime.UtcNow.AddDays(1);
        var slot = new TimeSlot(start, start.AddHours(1));
        var seats = new SeatCount(3);

        var session = new Session("Yoga", slot, seats);

        Assert.NotEqual(Guid.Empty, session.Id);
        Assert.Equal("Yoga", session.Title);
        Assert.Equal(slot, session.TimeSlot);
        Assert.Equal(seats, session.SeatCount);
        Assert.Empty(session.Bookings);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_BlankTitle_Throws(string? title)
    {
        var start = DateTime.UtcNow.AddDays(1);

        Assert.Throws<ArgumentException>(() => new Session(title!, new TimeSlot(start, start.AddHours(1)), new SeatCount(1)));
    }

    [Fact]
    public void Constructor_NullTimeSlot_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new Session("Yoga", null!, new SeatCount(1)));
    }

    [Fact]
    public void Constructor_NullSeatCount_Throws()
    {
        var start = DateTime.UtcNow.AddDays(1);

        Assert.Throws<ArgumentNullException>(() => new Session("Yoga", new TimeSlot(start, start.AddHours(1)), null!));
    }

    [Fact]
    public void Book_CreatesBookingWithSessionIdEmailAndTimestamp()
    {
        var session = CreateSession();

        var booking = session.Book(new EmailAddress("Sam@Example.com"));

        Assert.Equal(session.Id, booking.SessionId);
        Assert.Equal("sam@example.com", booking.EmailAddress.Value);
        Assert.InRange(booking.CreatedAt, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        Assert.Contains(booking, session.Bookings);
    }

    [Fact]
    public void CancelBooking_AlreadyCancelled_Throws()
    {
        var session = CreateSession();
        var booking = session.Book(new EmailAddress("a@example.com"));
        session.CancelBooking(booking.Id);

        Assert.Throws<InvalidOperationException>(() => session.CancelBooking(booking.Id));
    }

    [Fact]
    public void PromoteFromWaitlist_UnknownBooking_Throws()
    {
        var session = CreateSession();

        Assert.Throws<InvalidOperationException>(() => session.PromoteFromWaitlist(Guid.NewGuid()));
    }

    [Fact]
    public void PromoteFromWaitlist_BookingAlreadyConfirmed_Throws()
    {
        var session = CreateSession(capacity: 2);
        var confirmed = session.Book(new EmailAddress("a@example.com"));

        Assert.Throws<InvalidOperationException>(() => session.PromoteFromWaitlist(confirmed.Id));
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllRaisedEvents()
    {
        var session = CreateSession();
        session.Book(new EmailAddress("a@example.com"));
        Assert.NotEmpty(session.DomainEvents);

        session.ClearDomainEvents();

        Assert.Empty(session.DomainEvents);
    }

    [Fact]
    public void Book_SameEmailDifferentCase_Throws()
    {
        var session = CreateSession();
        session.Book(new EmailAddress("sam@example.com"));

        Assert.Throws<InvalidOperationException>(() => session.Book(new EmailAddress("SAM@Example.com")));
    }
}
