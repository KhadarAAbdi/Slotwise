using Slotwise.Application.Abstractions;
using Slotwise.Application.Sessions.DTOs;
using Slotwise.Application.Sessions.Interfaces;

namespace Slotwise.Application.Sessions.Queries.GetSession
{
    public class GetSessionQueryHandler : IQueryHandler<GetSessionQuery, SessionDTO>
    {
        private readonly ISessionQueries _queries;

        public GetSessionQueryHandler(ISessionQueries queries)
        {
            _queries = queries;
        }

        public async Task<SessionDTO> Handle(GetSessionQuery query)
        {
            return await _queries.GetByIdAsync(query.SessionId)
                ?? throw new SessionNotFoundException(query.SessionId);
        }
    }
}
