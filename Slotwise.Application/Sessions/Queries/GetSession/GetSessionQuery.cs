namespace Slotwise.Application.Sessions.Queries.GetSession
{
    public sealed record class GetSessionQuery
    {
        public Guid SessionId { get; }
        public GetSessionQuery(Guid sessionId)
        {
            SessionId = sessionId;
        }
    }
}
