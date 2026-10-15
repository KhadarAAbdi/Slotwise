using Slotwise.Domain.Sessions.Interfaces;

namespace Slotwise.Application.Abstractions
{
    /// <summary>
    /// Delivers domain events to every <see cref="IDomainEventHandler{TEvent}"/> registered for them.
    /// Needed so the code that saves an aggregate can publish its events without knowing who listens.
    /// </summary>
    public interface IDomainEventDispatcher
    {
        Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents);
    }
}
