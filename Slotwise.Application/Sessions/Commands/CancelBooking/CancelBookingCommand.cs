namespace Slotwise.Application.Sessions.Commands.CancelBooking
{
    public sealed record CancelBookingCommand(Guid SessionId, Guid BookingId);
}
