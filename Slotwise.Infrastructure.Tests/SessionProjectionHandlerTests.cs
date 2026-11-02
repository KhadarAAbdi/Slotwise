using MongoDB.Driver;
using Slotwise.Domain.Sessions.Entities;
using Slotwise.Domain.Sessions.Events;
using Slotwise.Domain.Sessions.Interfaces;
using Slotwise.Domain.Sessions.ValueObjects;
using Slotwise.Infrastructure.Persistence.Mongo;
using Slotwise.Infrastructure.Persistence.Queries;
using Slotwise.Infrastructure.Persistence.Repositories;
using Slotwise.Infrastructure.Projections;

namespace Slotwise.Infrastructure.Tests;

[Collection(SqlServerCollection.Name)]
public class SessionProjectionHandlerTests(SqlServerFixture db)
{
    private const string ServerVariable = "SLOTWISE_TEST_MONGO";
    private const string DefaultServer = "mongodb://localhost:27017";

    private static Session NewSession(int capacity)
    {
        var start = DateTime.UtcNow.AddDays(1);
        return new Session("Yoga", new TimeSlot(start, start.AddHours(1)), new SeatCount(capacity));
    }

    private static IMongoCollection<SessionDocument> Sessions(IMongoDatabase mongo)
    {
        return mongo.GetCollection<SessionDocument>(SessionDocument.CollectionName);
    }

    private async Task SaveNewAsync(Session session)
    {
        var context = db.CreateContext();

        try
        {
            var repository = new SessionRepository(context, new RecordingDomainEventDispatcher());
            repository.Add(session);
            await repository.SaveChangesAsync();
        }
        finally
        {
            if (context is not null)
            {
                await context.DisposeAsync();
            }
        }
    }

    private async Task BookAsync(Guid sessionId, string email)
    {
        var context = db.CreateContext();

        try
        {
            var repository = new SessionRepository(context, new RecordingDomainEventDispatcher());
            var loaded = await repository.GetByIdAsync(sessionId);
            loaded!.Book(new EmailAddress(email));
            await repository.SaveChangesAsync();
        }
        finally
        {
            if (context is not null)
            {
                await context.DisposeAsync();
            }
        }
    }

    private async Task ProjectAsync<TEvent>(IMongoDatabase mongo, TEvent domainEvent) where TEvent : IDomainEvent
    {
        var context = db.CreateContext();

        try
        {
            await new SessionProjectionHandler<TEvent>(new SessionQueries(context), mongo).Handle(domainEvent);
        }
        finally
        {
            if (context is not null)
            {
                await context.DisposeAsync();
            }
        }
    }

    [Fact]
    public async Task Handle_CopiesTheCurrentSqlStateIntoMongo()
    {
        var client = new MongoClient(Environment.GetEnvironmentVariable(ServerVariable) ?? DefaultServer);
        var databaseName = "SlotwiseTests_" + Guid.NewGuid().ToString("N");

        try
        {
            var mongo = client.GetDatabase(databaseName);
            var session = NewSession(capacity: 1);
            session.Book(new EmailAddress("a@example.com"));
            session.Book(new EmailAddress("b@example.com"));
            await SaveNewAsync(session);

            await ProjectAsync(mongo, new SessionCreated(session.Id));

            var document = await Sessions(mongo).Find(d => d.Id == session.Id).SingleAsync();
            Assert.Equal(1, document.ConfirmedCount);
            Assert.Equal(1, document.WaitlistCount);
            Assert.Equal(2, document.Bookings.Count);
        }
        finally
        {
            await client.DropDatabaseAsync(databaseName);
            client.Dispose();
        }
    }

    [Fact]
    public async Task Handle_CalledAgainAfterAChange_ReplacesTheDocumentInsteadOfAddingAnother()
    {
        var client = new MongoClient(Environment.GetEnvironmentVariable(ServerVariable) ?? DefaultServer);
        var databaseName = "SlotwiseTests_" + Guid.NewGuid().ToString("N");

        try
        {
            var mongo = client.GetDatabase(databaseName);
            var session = NewSession(capacity: 2);
            await SaveNewAsync(session);
            await ProjectAsync(mongo, new SessionCreated(session.Id));

            await BookAsync(session.Id, "a@example.com");
            await ProjectAsync(mongo, new BookingConfirmed(session.Id, Guid.NewGuid(), "ignored@example.com"));

            var documents = await Sessions(mongo).Find(FilterDefinition<SessionDocument>.Empty).ToListAsync();
            var document = Assert.Single(documents);
            Assert.Equal(1, document.ConfirmedCount);
        }
        finally
        {
            await client.DropDatabaseAsync(databaseName);
            client.Dispose();
        }
    }
}
