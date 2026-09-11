using Slotwise.Domain.Base;
using Slotwise.Domain.Sessions.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        
        public Session(string title, TimeSlot timeSlot, SeatCount seatCount)
        {
            Title = title; 
            TimeSlot = timeSlot; 
            SeatCount = seatCount;
        }

    }
}
