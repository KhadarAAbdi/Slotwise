using Slotwise.Domain.Sessions.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Slotwise.Domain.Sessions.Events
{
    /// <summary>
    /// Raised when a booking is created but the session is full, so it joins the waitlist.
    /// Needed so anything mirroring the session hears about waitlisted bookings, which would otherwise change it without any event.
    /// </summary>
    public sealed record class BookingWaitlisted : IDomainEvent
    {
        /// <summary>The session the booking belongs to.</summary>
        public Guid SessionId { get; }

        /// <summary>The booking that was waitlisted.</summary>
        public Guid BookingId { get; }

        /// <summary>The attendee's email address.</summary>
        public string AttendeeEmail { get; }

        /// <summary>When the event occurred.</summary>
        public DateTime OccurredOn { get; }

        /// <summary>Creates the event, stamping the current UTC time.</summary>
        public BookingWaitlisted(Guid sessionId, Guid bookingId, string attendeeEmail)
        {
            SessionId = sessionId;
            BookingId = bookingId;
            AttendeeEmail = attendeeEmail;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
