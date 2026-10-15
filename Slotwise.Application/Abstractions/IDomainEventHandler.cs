using Slotwise.Domain.Sessions.Interfaces;

namespace Slotwise.Application.Abstractions
{
    /// <summary>
    /// Reacts to one kind of domain event, e.g. "when a booking is confirmed, update the read model".
    /// Needed so each reaction is its own small class that can be added without touching the aggregate
    /// or the command handlers that caused the event.
    /// </summary>
    public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
    {
        Task Handle(TEvent domainEvent);
    }
}
