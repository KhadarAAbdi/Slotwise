using Slotwise.Domain.Base;
using Slotwise.Domain.Sessions.ValueObjects;

namespace Slotwise.Domain.Sessions.Entities
{
    public class Session : AggregateRoot
    {
        public string Title { get; private set;} = string.Empty;
        public TimeSlot TimeSlot { get; private set; }
        public SeatCount SeatCount { get; private set; }

        private readonly List<Booking> bookings = new List<Booking>();
        public IReadOnlyList<Booking> Bookings { get { return bookings.AsReadOnly(); } }

        private Session() {  }
        
        public Session(string title, TimeSlot timeSlot, SeatCount seatCount) : base(Guid.NewGuid())
        {
            if (string.IsNullOrWhiteSpace(Title))
                throw new ArgumentNullException("Title is required.", nameof(title));

            Title = title; 
            TimeSlot = timeSlot; 
            SeatCount = seatCount;
        }

        private int ConfirmedCount()
        {
            return bookings.Count(b => b.Status == BookingStatus.Confirmed);
        }

    }
}
