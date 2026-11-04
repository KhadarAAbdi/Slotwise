using Slotwise.Application.Abstractions;
using Slotwise.Domain.Sessions.Interfaces;

namespace Slotwise.Infrastructure.Tests;

/// <summary>
/// A stand-in dispatcher that records what it was asked to deliver, so tests can assert on
/// which events a save published without needing any real handlers.
/// </summary>
public sealed class RecordingDomainEventDispatcher : IDomainEventDispatcher
{
    public List<IDomainEvent> Dispatched { get; } = new();

    public Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents)
    {
        Dispatched.AddRange(domainEvents);
        return Task.CompletedTask;
    }
}
