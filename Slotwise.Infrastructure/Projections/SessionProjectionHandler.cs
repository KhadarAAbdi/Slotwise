using MongoDB.Driver;
using Slotwise.Application.Abstractions;
using Slotwise.Domain.Sessions.Interfaces;
using Slotwise.Infrastructure.Persistence.Mongo;
using Slotwise.Infrastructure.Persistence.Queries;

namespace Slotwise.Infrastructure.Projections
{
    /// <summary>
    /// Keeps the Mongo read model in step with SQL Server. Any event it handles means "this session changed",
    /// so it ignores what the event carries, re-reads the whole session from SQL, and replaces the document.
    /// Replacing (an upsert) makes it safe to run twice and safe if events arrive out of order.
    /// </summary>
    public sealed class SessionProjectionHandler<TEvent> : IDomainEventHandler<TEvent> where TEvent : IDomainEvent
    {
        private readonly SessionQueries _sqlQueries;
        private readonly IMongoCollection<SessionDocument> _sessions;

        public SessionProjectionHandler(SessionQueries sqlQueries, IMongoDatabase database)
        {
            _sqlQueries = sqlQueries;
            _sessions = database.GetCollection<SessionDocument>(SessionDocument.CollectionName);
        }

        public async Task Handle(TEvent domainEvent)
        {
            var session = await _sqlQueries.GetByIdAsync(domainEvent.SessionId);
            if (session is null)
            {
                return;
            }

            await _sessions.ReplaceOneAsync(
                d => d.Id == domainEvent.SessionId,
                SessionDocument.From(session),
                new ReplaceOptions { IsUpsert = true });
        }
    }
}
