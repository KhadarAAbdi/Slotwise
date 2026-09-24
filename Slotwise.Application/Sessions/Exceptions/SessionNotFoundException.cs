namespace Slotwise.Application.Sessions.Exceptions
{
    public sealed class SessionNotFoundException : Exception
    {
        public SessionNotFoundException(Guid sessionId)
            : base($"Session '{sessionId}' was not found.")
        {
        }
    }
}
