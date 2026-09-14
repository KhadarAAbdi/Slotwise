using System;
using Slotwise.Domain.Sessions.Interfaces;

namespace Slotwise.Domain.Sessions.Events
{
    /// <summary>
    /// Raised when a waitlisted booking is promoted into a confirmed seat.
    /// Needed as its own event (distinct from <see cref="BookingConfirmed"/>) so a consumer can
    /// tell "this attendee booked directly" apart from "this attendee just got in off the waitlist" —
    /// the latter is what should trigger a "you're in!" notification.
    /// </summary>
    public sealed record class WaitlistPromoted : IDomainEvent
    {
        /// <summary>The session the booking belongs to.</summary>
        public Guid SessionId { get; }

        /// <summary>The booking that was promoted.</summary>
        public Guid BookingId { get; }

        /// <summary>The attendee's email address.</summary>
        public string AttendeeEmail { get; }

        /// <summary>When the event occurred.</summary>
        public DateTime OccurredOn { get; }

        /// <summary>Creates the event, stamping the current UTC time.</summary>
        public WaitlistPromoted(Guid sessionId, Guid bookingId, string attendeeEmail)
        {
            SessionId = sessionId;
            BookingId = bookingId;
            AttendeeEmail = attendeeEmail;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
