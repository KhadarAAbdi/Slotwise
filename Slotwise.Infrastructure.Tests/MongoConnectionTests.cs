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

    private static IConfiguration BuildConfiguration(params (string Key, string Value)[] settings)
    {
        var values = settings.ToDictionary(s => s.Key, s => (string?)s.Value);
        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }

    [Fact]
    public async Task RegisteredClient_CanWriteAndReadBackADocument()
    {
        var server = Environment.GetEnvironmentVariable(ServerVariable) ?? DefaultServer;
        var configuration = BuildConfiguration(
            ("ConnectionStrings:SlotwiseDb", "Server=unused"),
            ("ConnectionStrings:MongoDb", server));

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
        var configuration = BuildConfiguration(("ConnectionStrings:SlotwiseDb", "Server=unused"));

        var exception = Assert.Throws<InvalidOperationException>(
            () => new ServiceCollection().AddInfrastructure(configuration));

        Assert.Contains("MongoDb", exception.Message);
    }
}
