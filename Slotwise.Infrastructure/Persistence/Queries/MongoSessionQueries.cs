using MongoDB.Driver;
using Slotwise.Application.Sessions.DTOs;
using Slotwise.Application.Sessions.Interfaces;
using Slotwise.Infrastructure.Persistence.Mongo;

namespace Slotwise.Infrastructure.Persistence.Queries
{
    /// <summary>
    /// The read side served from the Mongo read model: each query is a single document lookup, with no joins.
    /// Takes the database rather than the client so it never needs to know the database name.
    /// </summary>
    public class MongoSessionQueries : ISessionQueries
    {
        private readonly IMongoCollection<SessionDocument> _sessions;

        public MongoSessionQueries(IMongoDatabase database)
        {
            _sessions = database.GetCollection<SessionDocument>(SessionDocument.CollectionName);
        }

        public async Task<SessionDTO?> GetByIdAsync(Guid id)
        {
            var document = await _sessions.Find(d => d.Id == id).FirstOrDefaultAsync();

            return document?.ToDto();
        }

        public async Task<IReadOnlyList<SessionDTO>> ListAsync()
        {
            var documents = await _sessions
                .Find(FilterDefinition<SessionDocument>.Empty)
                .SortBy(d => d.Start)
                .ToListAsync();

            return documents.Select(d => d.ToDto()).ToList();
        }
    }
}
