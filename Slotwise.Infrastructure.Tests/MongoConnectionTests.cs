using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Slotwise.Infrastructure.Tests;

/// <summary>
/// Proves the Mongo wiring end to end: AddInfrastructure reads the connection string, the client it
/// registers reaches a real server, and a document can be written and read back.
/// The server comes from SLOTWISE_TEST_MONGO, defaulting to a local container.
/// </summary>
public class MongoConnectionTests
{
    private const string ServerVariable = "SLOTWISE_TEST_MONGO";
    private const string DefaultServer = "mongodb://localhost:27017";

    private static IConfiguration BuildConfiguration(Dictionary<string,string?> settings)
    {
        return new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
    }

    [Fact]
    public async Task RegisteredClient_CanWriteAndReadBackADocument()
    {
        Dictionary<string,string?> settings = new()
        {
            { "ConnectionStrings:SlotwiseDb", "Server=unused" },
            { "ConnectionStrings:MongoDb", Environment.GetEnvironmentVariable(ServerVariable) ?? DefaultServer }
        };
        var server = Environment.GetEnvironmentVariable(ServerVariable) ?? DefaultServer;
        var configuration = BuildConfiguration(settings);

        var provider = new ServiceCollection().AddInfrastructure(configuration).BuildServiceProvider();
        var client = provider.GetRequiredService<IMongoClient>();
        var databaseName = "SlotwiseTests_" + Guid.NewGuid().ToString("N");

        try
        {
            var collection = client.GetDatabase(databaseName).GetCollection<BsonDocument>("smoke");
            var id = Guid.NewGuid().ToString();

            await collection.InsertOneAsync(new BsonDocument { { "_id", id }, { "title", "hello" } });
            var found = await collection.Find(new BsonDocument("_id", id)).SingleOrDefaultAsync();

            Assert.NotNull(found);
            Assert.Equal("hello", found["title"].AsString);
        }
        finally
        {
            await client.DropDatabaseAsync(databaseName);
            await provider.DisposeAsync();
        }
    }

    [Fact]
    public void AddInfrastructure_WithoutMongoConnectionString_FailsFast()
    {
        Dictionary<string, string?> settings = new()
        {
            { "ConnectionStrings:SlotwiseDb", "Server=unused" },
        };

        var configuration = BuildConfiguration(settings);

        var exception = Assert.Throws<InvalidOperationException>(
            () => new ServiceCollection().AddInfrastructure(configuration));

        Assert.Contains("MongoDb", exception.Message);
    }
}
