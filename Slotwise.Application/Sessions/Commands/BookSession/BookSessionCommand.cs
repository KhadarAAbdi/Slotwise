namespace Slotwise.Application.Sessions.Commands.BookSession
{
    public sealed record BookSessionCommand(Guid SessionId, string Email);
}
