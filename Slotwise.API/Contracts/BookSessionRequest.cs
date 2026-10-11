namespace Slotwise.API.Contracts
{
    public sealed record class BookSessionRequest
    {
        public string Email { get; init; } = string.Empty;
    }
}
