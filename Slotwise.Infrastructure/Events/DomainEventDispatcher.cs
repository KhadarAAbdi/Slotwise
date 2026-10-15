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
                var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
                var handleMethod = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.Handle))!;

                foreach (var handler in _services.GetServices(handlerType))
                {
                    try
                    {
                        await (Task)handleMethod.Invoke(handler, new object[] { domainEvent })!;
                    }
                    catch (Exception ex)
                    {
                        // Reflection wraps whatever the handler threw; log the real exception.
                        var cause = ex is System.Reflection.TargetInvocationException { InnerException: not null } wrapped
                            ? wrapped.InnerException!
                            : ex;

                        _logger.LogError(cause, "Handler {Handler} failed for event {EventType}.", handler!.GetType().Name, eventType.Name);
                    }
                }
            }
        }
    }
}
