using Slotwise.Domain.Sessions.Interfaces;

namespace Slotwise.Domain.Base
{
    /// <summary>
    /// Base class for an aggregate root — the only entity within a cluster of related
    /// objects (the "aggregate") that outside code is allowed to reference directly.
    /// Needed to enforce the DDD rule that all changes to child entities (like <see cref="Slotwise.Domain.Sessions.Entities.Booking"/>)
    /// go through the root, which is what makes it possible to guarantee invariants like the capacity limit.
    /// Also collects the domain events raised while those invariants are enforced,
    /// so infrastructure code can publish them after the aggregate is persisted.
    /// </summary>
    public abstract class AggregateRoot : Entity
    {
        private readonly List<IDomainEvent> _domainEvents = new List<IDomainEvent>();

        /// <summary>
        /// Domain events raised on this aggregate since it was loaded or since the last <see cref="ClearDomainEvents"/> call.
        /// Exposed read-only so outside code can read and publish events but can't inject its own into the aggregate.
        /// </summary>
        public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        /// <summary>Parameterless constructor reserved for ORMs (e.g. EF Core), which materialize aggregates via reflection and never call the public constructor.</summary>
        protected AggregateRoot() { }

        /// <summary>Creates an aggregate root with the given identity.</summary>
        protected AggregateRoot(Guid id) : base(id)
        {
        }

        /// <summary>
        /// Records a domain event that occurred as a result of a state change on this aggregate.
        /// Protected so only the aggregate itself (never outside code) can decide when an event happened.
        /// </summary>
        protected void RaiseEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        /// <summary>
        /// Clears the recorded domain events. Needed so infrastructure can call this right after publishing
        /// them, preventing the same event from being re-published the next time this aggregate is saved.
        /// </summary>
        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}
