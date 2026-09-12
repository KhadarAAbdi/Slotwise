using Slotwise.Domain.Sessions.Entities;
using Slotwise.Domain.Sessions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slotwise.Domain.Sessions.Events
{

    public sealed record class BookingConfirmed : IDomainEvent
    {
        public Guid SessionId { get; }
        public Guid BookingId { get; }
        public string AttendeeEmail { get; }
        public DateTime OccuredOn { get; }

        public BookingConfirmed(Guid sessionId, Guid bookingId, string attendeeEmail)
        {
            SessionId = sessionId;
            BookingId = bookingId;
            AttendeeEmail = attendeeEmail;
            OccuredOn = DateTime.UtcNow;
        }
    }
}
