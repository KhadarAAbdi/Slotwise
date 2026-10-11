namespace Slotwise.API.Contracts
{
    public sealed record class CreateSessionRequest
    {
        public string Title { get; init; } = string.Empty;
        public DateTime Start { get; init; }
        public DateTime End { get; init; }
        public int Capacity { get; init; }
    }
}
