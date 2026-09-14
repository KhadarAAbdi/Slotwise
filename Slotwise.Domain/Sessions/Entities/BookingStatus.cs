namespace Slotwise.Domain.Sessions.Entities
{
    /// <summary>
    /// The lifecycle state of a <see cref="Booking"/>. An enum instead of a string or int so the
    /// set of valid states is closed and known at compile time, and the compiler can flag unhandled cases in a switch.
    /// </summary>
    public enum BookingStatus
    {
        /// <summary>The booking holds one of the session's confirmed seats. Counted against <see cref="Session.SeatCount"/> to enforce the capacity invariant.</summary>
        Confirmed,

        /// <summary>The session was full when this booking was made; it's queued for the next open seat. Needed so an over-capacity request isn't simply rejected but preserved in line.</summary>
        Waitlisted,

        /// <summary>The attendee cancelled; the booking no longer holds or awaits a seat. Kept as a terminal state (not deleted) so the booking's history is preserved.</summary>
        Cancelled
    }
}
