namespace Slotwise.Application.Sessions.Commands.CancelBooking
{
    public sealed record class CancelBookingCommand(Guid SessionId, Guid BookingId);
}
