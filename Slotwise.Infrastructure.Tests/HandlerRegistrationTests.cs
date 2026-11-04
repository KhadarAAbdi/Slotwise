using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Slotwise.Application.Abstractions;
using Slotwise.Domain.Sessions.Events;

namespace Slotwise.Infrastructure.Tests;

public class HandlerRegistrationTests
{
    [Fact]
    public void ProjectionHandler_IsRegisteredForTheFiveSessionEvents_ButNotSpotOpened()
    {
        var settings = new Dictionary<string, string?>
        {
            { "ConnectionStrings:SlotwiseDb", "Server=unused" },
            { "ConnectionStrings:MongoDb", "mongodb://localhost:27017/slotwise" },
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var provider = new ServiceCollection()
            .AddLogging()
            .AddInfrastructure(configuration)
            .BuildServiceProvider();

        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;

        Assert.Single(services.GetServices<IDomainEventHandler<SessionCreated>>());
        Assert.Single(services.GetServices<IDomainEventHandler<BookingConfirmed>>());
        Assert.Single(services.GetServices<IDomainEventHandler<BookingWaitlisted>>());
        Assert.Single(services.GetServices<IDomainEventHandler<BookingCancelled>>());
        Assert.Single(services.GetServices<IDomainEventHandler<WaitlistPromoted>>());
        Assert.Empty(services.GetServices<IDomainEventHandler<SpotOpened>>());
    }
}
