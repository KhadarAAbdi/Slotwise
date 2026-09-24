using System;
using System.Threading.Tasks;
using Slotwise.Domain.Sessions.Entities;

namespace Slotwise.Application.Sessions.Interfaces
{
    /// <summary>
    /// Persistence contract for the <see cref="Session"/> aggregate. Implemented by the
    /// Infrastructure layer; the Domain layer only depends on this abstraction (Dependency Inversion),
    /// so the domain model never has to know EF Core or Postgres exist.
    /// </summary>
    public interface ISessionRepository
    {
        /// <summary>Loads a session by id, or null if none exists with that id. Needed so a use-case can fetch an existing aggregate before calling behavior on it.</summary>
        Task<Session?> GetByIdAsync(Guid id);

        /// <summary>Registers a new session to be persisted. Needed so a use-case can hand off a freshly created aggregate without knowing how storage works.</summary>
        Task AddAsync(Session session);

        /// <summary>Persists any pending changes to the underlying store. Kept separate from Add/Get so multiple changes can be batched into one save (unit-of-work style), matching how EF Core tracks changes.</summary>
        Task SaveChangesAsync();
    }
}
