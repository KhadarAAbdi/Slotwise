using MongoDB.Driver;
using Slotwise.Application.Sessions.DTOs;
using Slotwise.Infrastructure.Persistence.Mongo;
using Slotwise.Infrastructure.Persistence.Queries;

namespace Slotwise.Infrastructure.Tests;

public class MongoSessionQueriesTests
{
    private const string ServerVariable = "SLOTWISE_TEST_MONGO";
    private const string DefaultServer = "mongodb://localhost:27017";

    private static SessionDTO NewDto(string title, DateTime start)
    {
        return new SessionDTO(Guid.NewGuid(), title, start, start.AddHours(1), 2, 0, 0, new List<BookingDTO>());
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsTheStoredSession_AndNullWhenThereIsNone()
    {
        var client = new MongoClient(Environment.GetEnvironmentVariable(ServerVariable) ?? DefaultServer);
        var databaseName = "SlotwiseTests_" + Guid.NewGuid().ToString("N");

        try
        {
            var database = client.GetDatabase(databaseName);
            var stored = NewDto("Yoga", new DateTime(2030, 1, 15, 9, 0, 0, DateTimeKind.Utc));
            await database.GetCollection<SessionDocument>(SessionDocument.CollectionName)
                .InsertOneAsync(SessionDocument.From(stored));
            var queries = new MongoSessionQueries(database);

            var found = await queries.GetByIdAsync(stored.Id);
            var missing = await queries.GetByIdAsync(Guid.NewGuid());

            Assert.Equal("Yoga", found!.Title);
            Assert.Null(missing);
        }
        finally
        {
            await client.DropDatabaseAsync(databaseName);
            client.Dispose();
        }
    }

    [Fact]
    public async Task ListAsync_ReturnsSessionsOrderedByStart()
    {
        var client = new MongoClient(Environment.GetEnvironmentVariable(ServerVariable) ?? DefaultServer);
        var databaseName = "SlotwiseTests_" + Guid.NewGuid().ToString("N");

        try
        {
            var database = client.GetDatabase(databaseName);
            var later = NewDto("Later", new DateTime(2030, 3, 1, 9, 0, 0, DateTimeKind.Utc));
            var sooner = NewDto("Sooner", new DateTime(2030, 1, 1, 9, 0, 0, DateTimeKind.Utc));
            await database.GetCollection<SessionDocument>(SessionDocument.CollectionName)
                .InsertManyAsync(new[] { SessionDocument.From(later), SessionDocument.From(sooner) });

            var listed = await new MongoSessionQueries(database).ListAsync();

            Assert.Equal(new[] { "Sooner", "Later" }, listed.Select(s => s.Title));
        }
        finally
        {
            await client.DropDatabaseAsync(databaseName);
            client.Dispose();
        }
    }
}
