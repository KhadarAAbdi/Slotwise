using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Slotwise.Application.Abstractions;
using Slotwise.Domain.Sessions.Interfaces;

namespace Slotwise.Infrastructure.Events
{
    /// <summary>
    /// Delivers each event to the handlers registered for its type, in-process and in the same request.
    /// A handler that fails is logged and skipped rather than thrown: by the time events are dispatched the
    /// change is already committed, so failing the request would tell the caller their booking failed when it did not.
    /// The cost is that a failed handler leaves its target (e.g. the read model) stale; closing that gap
    /// reliably is what an outbox is for.
    /// </summary>
    public sealed class DomainEventDispatcher : IDomainEventDispatcher
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<DomainEventDispatcher> _logger;

        public DomainEventDispatcher(IServiceProvider services, ILogger<DomainEventDispatcher> logger)
        {
            _services = services;
            _logger = logger;
        }

        public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents)
        {
            foreach (var domainEvent in domainEvents)
            {
                // Handlers are registered against the concrete event type (IDomainEventHandler<BookingConfirmed>),
                // but all we hold is an IDomainEvent, so the handler type has to be built from the runtime type.
                var eventType = domainEvent.GetType();

                // One line per business event, with the session id only: attendee emails are personal data and stay out of the logs.
                _logger.LogInformation("Domain event {EventType} raised for session {SessionId}.", eventType.Name, domainEvent.SessionId);

                var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
                var handleMethod = handlerType.GetMethod("Handle")!;

                foreach (var handler in _services.GetServices(handlerType))
                {
                    try
                    {
                        await (Task)handleMethod.Invoke(handler, new object[] { domainEvent })!;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Handler {Handler} failed for event {EventType} on session {SessionId}. The change is saved in SQL, but the read model may be stale.", handler!.GetType().Name, eventType.Name, domainEvent.SessionId);
                    }
                }
            }
        }
    }
}
