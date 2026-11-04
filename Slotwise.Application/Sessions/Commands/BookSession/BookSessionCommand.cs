namespace Slotwise.Application.Sessions.Commands.BookSession
{
    public sealed record class BookSessionCommand(Guid SessionId, string Email);
}
