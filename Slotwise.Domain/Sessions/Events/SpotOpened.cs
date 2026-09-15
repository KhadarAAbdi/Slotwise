using System;
using Slotwise.Domain.Sessions.Interfaces;

namespace Slotwise.Domain.Sessions.Events
{
    /// <summary>
    /// Raised specifically when a cancellation frees up a confirmed seat (as opposed to a
    /// waitlisted booking being cancelled, which frees nothing). Needed as its own event —
    /// rather than reusing <see cref="BookingCancelled"/> — so a messaging worker can subscribe
    /// only to "capacity changed" without also reacting to waitlist cancellations that don't matter.
    /// </summary>
    public sealed record class SpotOpened : IDomainEvent
    {
        /// <summary>The session with a newly available seat.</summary>
        public Guid SessionId { get; }

        /// <summary>When the event occurred.</summary>
        public DateTime OccurredOn { get; }

        /// <summary>Creates the event, stamping the current UTC time.</summary>
        public SpotOpened(Guid sessionId)
        {
            SessionId = sessionId;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
