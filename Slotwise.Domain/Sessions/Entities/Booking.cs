using System;
using Slotwise.Domain.Base;
using Slotwise.Domain.Sessions.ValueObjects;

namespace Slotwise.Domain.Sessions.Entities
{
    /// <summary>
    /// One attendee's claim on a <see cref="Session"/>. A child entity of the <see cref="Session"/>
    /// aggregate — it has its own identity but its lifecycle and status transitions are only
    /// ever driven by the owning <see cref="Session"/>, never mutated directly from outside.
    /// Needed to keep the capacity invariant and event-raising logic in one place (the aggregate root)
    /// rather than letting a booking change its own status unchecked.
    /// </summary>
    public sealed class Booking : Entity
    {
        /// <summary>The session this booking belongs to. Needed since a booking doesn't hold a full back-reference to its <see cref="Session"/>, only the id.</summary>
        public Guid SessionId { get; private set; }

        /// <summary>The attendee's email address.</summary>
        public EmailAddress EmailAddress { get; private set; } = null!;

        /// <summary>The current lifecycle state of this booking. Drives whether it counts toward capacity and whether it's eligible for promotion.</summary>
        public BookingStatus Status { get; private set; }

        /// <summary>When this booking was created. Needed to order the waitlist FIFO (first waitlisted, first promoted).</summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>Parameterless constructor reserved for ORMs (e.g. EF Core), which materialize entities via reflection and never call the internal constructor.</summary>
        private Booking() { }

        /// <summary>
        /// Creates a booking. Internal because only <see cref="Session"/> (same assembly) is allowed
        /// to decide when a new booking is created and whether it starts out confirmed or waitlisted —
        /// that decision depends on capacity, which only the aggregate root can check safely.
        /// </summary>
        internal Booking(Guid sessionId, EmailAddress emailAddress, BookingStatus status) : base(Guid.NewGuid())
        {
            SessionId = sessionId;
            EmailAddress = emailAddress;
            Status = status;
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Marks the booking cancelled. Throws if it was already cancelled. Internal so cancellation
        /// always goes through <see cref="Session.CancelBooking"/>, which is what decides whether a
        /// <see cref="Events.SpotOpened"/> event needs to be raised alongside it.
        /// </summary>
        internal void Cancel()
        {
            if (Status == BookingStatus.Cancelled)
                throw new InvalidOperationException("Booking is already cancelled.");

            Status = BookingStatus.Cancelled;
        }

        /// <summary>
        /// Moves a waitlisted booking into confirmed. Throws if it wasn't waitlisted. Internal so
        /// promotion always goes through <see cref="Session.PromoteFromWaitlist"/>, which re-checks
        /// capacity first and raises the <see cref="Events.WaitlistPromoted"/> event.
        /// </summary>
        internal void Promote()
        {
            if (Status != BookingStatus.Waitlisted)
                throw new InvalidOperationException("Only a waitlisted booking can be promoted.");

            Status = BookingStatus.Confirmed;
        }
    }
}
