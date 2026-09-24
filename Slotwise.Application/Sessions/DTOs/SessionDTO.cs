using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slotwise.Application.Sessions.DTOs
{
    public sealed record class SessionDTO
    {
        public Guid Id { get; }
        public string Title { get; }
        public DateTime Start { get; }
        public DateTime End { get; }
        public int Capacity { get; }
        public int ConfirmedCount { get; }
        public int WaitlistCount { get; }
        public IReadOnlyList<BookingDTO> Bookings { get; }
        public SessionDTO(Guid id, string title, DateTime start, DateTime end, int capacity, int confirmedCount, int waitlistCount, IReadOnlyList<BookingDTO> bookings) 
        {
            Id = id;
            Title = title;
            Start = start;
            End = end;
            Capacity = capacity;
            ConfirmedCount = confirmedCount;
            WaitlistCount = waitlistCount;
            Bookings = bookings;
        }
    }
}
