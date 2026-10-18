using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Slotwise.Infrastructure.Tests;

public class MongoConnectionTests
{
    [Fact]
    public void AddInfrastructure_WithoutMongoConnectionString_FailsFast()
    {
        var settings = new Dictionary<string, string?>
        {
            { "ConnectionStrings:SlotwiseDb", "Server=unused" },
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        var exception = Assert.Throws<InvalidOperationException>(
            () => new ServiceCollection().AddInfrastructure(configuration));

        Assert.Contains("MongoDb", exception.Message);
    }
}
