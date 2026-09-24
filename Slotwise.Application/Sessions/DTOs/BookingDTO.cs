namespace Slotwise.Application.Sessions.DTOs
{
    public sealed record BookingDTO(Guid Id, string Email, string Status, DateTime CreatedAt);
}
