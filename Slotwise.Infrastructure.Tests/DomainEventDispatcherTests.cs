using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Slotwise.Application.Abstractions;
using Slotwise.Domain.Sessions.Events;
using Slotwise.Domain.Sessions.Interfaces;
using Slotwise.Infrastructure.Events;

namespace Slotwise.Infrastructure.Tests;

public class DomainEventDispatcherTests
{
    private sealed class RecordingHandler<TEvent> : IDomainEventHandler<TEvent> where TEvent : IDomainEvent
    {
        public List<TEvent> Received { get; } = new();

        public Task Handle(TEvent domainEvent)
        {
            Received.Add(domainEvent);
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingHandler : IDomainEventHandler<SpotOpened>
    {
        public Task Handle(SpotOpened domainEvent) => throw new InvalidOperationException("handler blew up");
    }

    private static DomainEventDispatcher CreateDispatcher(Action<IServiceCollection> register)
    {
        var services = new ServiceCollection();
        register(services);

        return new DomainEventDispatcher(
            services.BuildServiceProvider(),
            NullLogger<DomainEventDispatcher>.Instance);
    }

    [Fact]
    public async Task DispatchAsync_CallsEveryHandlerRegisteredForTheEventType()
    {
        var first = new RecordingHandler<BookingConfirmed>();
        var second = new RecordingHandler<BookingConfirmed>();
        var dispatcher = CreateDispatcher(services =>
        {
            services.AddSingleton<IDomainEventHandler<BookingConfirmed>>(first);
            services.AddSingleton<IDomainEventHandler<BookingConfirmed>>(second);
        });
        var evt = new BookingConfirmed(Guid.NewGuid(), Guid.NewGuid(), "a@example.com");

        await dispatcher.DispatchAsync(new IDomainEvent[] { evt });

        Assert.Same(evt, Assert.Single(first.Received));
        Assert.Same(evt, Assert.Single(second.Received));
    }

    [Fact]
    public async Task DispatchAsync_DoesNotCallHandlersForOtherEventTypes()
    {
        var spotOpenedHandler = new RecordingHandler<SpotOpened>();
        var dispatcher = CreateDispatcher(services =>
            services.AddSingleton<IDomainEventHandler<SpotOpened>>(spotOpenedHandler));

        await dispatcher.DispatchAsync(new IDomainEvent[] { new BookingConfirmed(Guid.NewGuid(), Guid.NewGuid(), "a@example.com") });

        Assert.Empty(spotOpenedHandler.Received);
    }

    [Fact]
    public async Task DispatchAsync_WithNoHandlerRegistered_DoesNothing()
    {
        var dispatcher = CreateDispatcher(_ => { });

        await dispatcher.DispatchAsync(new IDomainEvent[] { new SpotOpened(Guid.NewGuid()) });
    }

    [Fact]
    public async Task DispatchAsync_DeliversEveryEventInTheBatch()
    {
        var created = new RecordingHandler<SessionCreated>();
        var opened = new RecordingHandler<SpotOpened>();
        var dispatcher = CreateDispatcher(services =>
        {
            services.AddSingleton<IDomainEventHandler<SessionCreated>>(created);
            services.AddSingleton<IDomainEventHandler<SpotOpened>>(opened);
        });
        var sessionId = Guid.NewGuid();

        await dispatcher.DispatchAsync(new IDomainEvent[] { new SessionCreated(sessionId), new SpotOpened(sessionId) });

        Assert.Single(created.Received);
        Assert.Single(opened.Received);
    }

    [Fact]
    public async Task DispatchAsync_WhenAHandlerFails_StillRunsTheOthersAndDoesNotThrow()
    {
        var after = new RecordingHandler<SpotOpened>();
        var dispatcher = CreateDispatcher(services =>
        {
            services.AddSingleton<IDomainEventHandler<SpotOpened>, ThrowingHandler>();
            services.AddSingleton<IDomainEventHandler<SpotOpened>>(after);
        });

        await dispatcher.DispatchAsync(new IDomainEvent[] { new SpotOpened(Guid.NewGuid()) });

        Assert.Single(after.Received);
    }
}
