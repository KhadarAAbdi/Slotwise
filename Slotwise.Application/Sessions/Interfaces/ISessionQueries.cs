using Slotwise.Application.Sessions.DTOs;

namespace Slotwise.Application.Sessions.Interfaces
{
    /// <summary>
    /// Read-side contract. Implemented in Infrastructure by projecting straight from the
    /// database into DTOs, so queries never load or mutate the Session aggregate.
    /// </summary>
    public interface ISessionQueries
    {
        Task<SessionDTO?> GetByIdAsync(Guid id);

        Task<IReadOnlyList<SessionDTO>> ListAsync();
    }
}
