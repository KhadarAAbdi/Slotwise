using Slotwise.Application.Abstractions;
using Slotwise.Application.Sessions.DTOs;
using Slotwise.Application.Sessions.Interfaces;

namespace Slotwise.Application.Sessions.Queries.ListSessions
{
    public class ListSessionsQueryHandler : IQueryHandler<ListSessionsQuery, IReadOnlyList<SessionDTO>>
    {
        private readonly ISessionQueries _queries;

        public ListSessionsQueryHandler(ISessionQueries queries)
        {
            _queries = queries;
        }

        public Task<IReadOnlyList<SessionDTO>> Handle(ListSessionsQuery query)
        {
            return _queries.ListAsync();
        }
    }
}
