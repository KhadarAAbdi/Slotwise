using System;

namespace Slotwise.Domain.Sessions.Interfaces
{
    /// <summary>
    /// Marker contract for something that happened in the domain and other parts of the system may care about.
    /// Needed so <see cref="Base.AggregateRoot"/> and any future publisher/worker can handle every event type
    /// (BookingConfirmed, SpotOpened, etc.) through one shared shape, without knowing about each concrete event class.
    /// </summary>
    public interface IDomainEvent
    {
        /// <summary>The UTC timestamp at which the event occurred. Needed for ordering and auditing events consistently across every event type.</summary>
        DateTime OccurredOn { get; }
    }
}
