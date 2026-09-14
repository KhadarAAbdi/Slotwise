using System;
using Slotwise.Domain.Sessions.Interfaces;

namespace Slotwise.Domain.Sessions.Events
{
    /// <summary>
    /// Raised when a booking is created with an immediately available seat.
    /// A record because domain events are immutable facts about something that already happened —
    /// records give value equality and concise syntax for exactly that kind of data.
    /// </summary>
    public sealed record class BookingConfirmed : IDomainEvent
    {
        /// <summary>The session the booking belongs to. Needed so a consumer of this event (e.g. a future notification worker) knows which session it relates to.</summary>
        public Guid SessionId { get; }

        /// <summary>The booking that was confirmed.</summary>
        public Guid BookingId { get; }

        /// <summary>The attendee's email address. Carried on the event so a consumer doesn't need to re-query the session just to send a confirmation.</summary>
        public string AttendeeEmail { get; }

        /// <summary>When the event occurred.</summary>
        public DateTime OccurredOn { get; }

        /// <summary>Creates the event, stamping the current UTC time.</summary>
        public BookingConfirmed(Guid sessionId, Guid bookingId, string attendeeEmail)
        {
            SessionId = sessionId;
            BookingId = bookingId;
            AttendeeEmail = attendeeEmail;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
