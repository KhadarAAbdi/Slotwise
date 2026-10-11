namespace Slotwise.Application.Sessions.Exceptions
{
    public sealed class SessionConcurrencyException : Exception
    {
        public SessionConcurrencyException(Exception innerException)
            : base("The session was changed by another request. Reload it and try again.", innerException)
        {
        }
    }
}
