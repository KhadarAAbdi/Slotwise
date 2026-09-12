using Slotwise.Domain.Base;
using Slotwise.Domain.Sessions.ValueObjects;

namespace Slotwise.Domain.Sessions.Entities
{
    public class Booking : Entity
    {
        public Guid SessionId { get; private set; }
        public EmailAdress EmailAdress { get; private set; }
        public BookingStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private Booking(){}

        internal Booking(Guid sessionId, EmailAdress emailAdress, BookingStatus status) : base(Guid.NewGuid())
        {
            SessionId = sessionId;
            EmailAdress = emailAdress;
            Status = status;
            CreatedAt = DateTime.UtcNow;
        }

        internal void Cancel()
        {
            if (Status == BookingStatus.Cancelled)
            {
                throw new InvalidOperationException("Booking is already cancelled.");
            }
            Status = BookingStatus.Cancelled;
        }

        internal void Promote()
        {
            if (Status != BookingStatus.Waitlisted)
            {
                throw new InvalidOperationException("Only a waitlisted booking can be promoted.");
            }
            Status = BookingStatus.Confirmed;
        }
    }
}
