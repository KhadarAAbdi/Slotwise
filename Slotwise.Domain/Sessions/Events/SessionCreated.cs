using System;
using Slotwise.Domain.Sessions.Interfaces;

namespace Slotwise.Domain.Sessions.Events
{
    /// <summary>
    /// Raised when a brand-new session is created. Needed so anything that mirrors sessions elsewhere
    /// (e.g. the read model) learns about a session from the moment it exists, not only once someone books it.
    /// A record for the same reason as the other events: it is an immutable fact about something that already happened.
    /// </summary>
    public sealed record class SessionCreated : IDomainEvent
    {
        /// <summary>The session that was created. Consumers use it to load the session's current state.</summary>
        public Guid SessionId { get; }

        /// <summary>When the event occurred.</summary>
        public DateTime OccurredOn { get; }

        /// <summary>Creates the event, stamping the current UTC time.</summary>
        public SessionCreated(Guid sessionId)
        {
            SessionId = sessionId;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
