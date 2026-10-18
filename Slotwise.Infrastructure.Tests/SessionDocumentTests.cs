using MongoDB.Bson;
using MongoDB.Driver;
using Slotwise.Application.Sessions.DTOs;
using Slotwise.Infrastructure.Persistence.Mongo;

namespace Slotwise.Infrastructure.Tests;

public class SessionDocumentTests
{
    private const string ServerVariable = "SLOTWISE_TEST_MONGO";
    private const string DefaultServer = "mongodb://localhost:27017";

    // Whole milliseconds only: Mongo stores dates to the millisecond, so finer ticks would not survive a round trip.
    // Kind is left Unspecified on purpose, because that is what SQL Server returns.
    private static SessionDTO SampleDto()
    {
        var start = new DateTime(2030, 1, 15, 9, 0, 0, DateTimeKind.Unspecified);

        return new SessionDTO(
            Guid.NewGuid(),
            "Yoga",
            start,
            start.AddHours(1),
            capacity: 1,
            confirmedCount: 1,
            waitlistCount: 1,
            new List<BookingDTO>
            {
                new(Guid.NewGuid(), "ann@example.com", "Confirmed", start.AddDays(-2)),
                new(Guid.NewGuid(), "bob@example.com", "Waitlisted", start.AddDays(-1))
            });
    }

    [Fact]
    public async Task Document_RoundTripsThroughMongo_AndStoresIdsAsStandardUuids()
    {
        var client = new MongoClient(Environment.GetEnvironmentVariable(ServerVariable) ?? DefaultServer);
        var databaseName = "SlotwiseTests_" + Guid.NewGuid().ToString("N");

        try
        {
            var database = client.GetDatabase(databaseName);
            var collection = database.GetCollection<SessionDocument>("sessions");
            var dto = SampleDto();

            await collection.InsertOneAsync(SessionDocument.From(dto));
            var stored = (await collection.Find(d => d.Id == dto.Id).SingleAsync()).ToDto();

            Assert.Equal(dto.Id, stored.Id);
            Assert.Equal(dto.Title, stored.Title);
            Assert.Equal(dto.Start, stored.Start);
            Assert.Equal(dto.ConfirmedCount, stored.ConfirmedCount);
            Assert.Equal(dto.WaitlistCount, stored.WaitlistCount);
            Assert.Equal(dto.Bookings, stored.Bookings);

            var raw = await database.GetCollection<BsonDocument>("sessions")
                .Find(FilterDefinition<BsonDocument>.Empty)
                .SingleAsync();
            Assert.Equal(BsonBinarySubType.UuidStandard, raw["_id"].AsBsonBinaryData.SubType);
        }
        finally
        {
            await client.DropDatabaseAsync(databaseName);
            client.Dispose();
        }
    }

    [Fact]
    public void Mapping_MarksDatesAsUtcWithoutMovingThem()
    {
        var dto = SampleDto();

        var document = SessionDocument.From(dto);

        Assert.Equal(DateTimeKind.Utc, document.Start.Kind);
        Assert.Equal(dto.Start.Ticks, document.Start.Ticks);
    }
}
