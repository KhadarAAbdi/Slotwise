using System;
using Slotwise.Domain.Sessions.Interfaces;

namespace Slotwise.Domain.Sessions.Events
{
    /// <summary>
    /// Raised whenever a booking (confirmed or waitlisted) is cancelled.
    /// Kept separate from <see cref="SpotOpened"/> because "a booking was cancelled" and
    /// "a confirmed seat became available" are different facts — only the latter should trigger waitlist promotion.
    /// </summary>
    public sealed record class BookingCancelled : IDomainEvent
    {
        /// <summary>The session the booking belongs to.</summary>
        public Guid SessionId { get; }

        /// <summary>The booking that was cancelled.</summary>
        public Guid BookingId { get; }

        /// <summary>The attendee's email address.</summary>
        public string AttendeeEmail { get; }

        /// <summary>When the event occurred.</summary>
        public DateTime OccurredOn { get; }

        /// <summary>Creates the event, stamping the current UTC time.</summary>
        public BookingCancelled(Guid sessionId, Guid bookingId, string attendeeEmail)
        {
            SessionId = sessionId;
            BookingId = bookingId;
            AttendeeEmail = attendeeEmail;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
