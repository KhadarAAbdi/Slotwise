using Microsoft.EntityFrameworkCore;
using Slotwise.Application.Sessions.DTOs;
using Slotwise.Application.Sessions.Exceptions;
using Slotwise.Application.Sessions.Interfaces;
using Slotwise.Domain.Sessions.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Slotwise.Infrastructure.Persistence.Queries
{
    public class SessionQueries : ISessionQueries
    {
        private readonly SlotwiseDbContext _context;

        public SessionQueries(SlotwiseDbContext context)
        {
            _context = context;
        }

        public async Task<SessionDTO?> GetByIdAsync(Guid id)
        {
            return await _context.Sessions
                .Where(s => s.Id == id)
                .Select(s => new SessionDTO(
                    s.Id,
                    s.Title,
                    s.TimeSlot.Start,
                    s.TimeSlot.End,
                    s.SeatCount.Value,
                    s.Bookings.Count(b => b.Status == BookingStatus.Confirmed),
                    s.Bookings.Count(b => b.Status == BookingStatus.Waitlisted),
                    s.Bookings.Select(b => new BookingDTO(b.Id, b.EmailAddress.Value, b.Status.ToString(), b.CreatedAt)).ToList()
                ))
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<SessionDTO>> ListAsync()
        {
            return await _context.Sessions
                .Select(s => new SessionDTO(
                    s.Id,
                    s.Title,
                    s.TimeSlot.Start,
                    s.TimeSlot.End,
                    s.SeatCount.Value,
                    s.Bookings.Count(b => b.Status == BookingStatus.Confirmed),
                    s.Bookings.Count(b => b.Status == BookingStatus.Waitlisted),
                    s.Bookings.Select(b => new BookingDTO(b.Id, b.EmailAddress.Value, b.Status.ToString(), b.CreatedAt)).ToList()
                ))
                .ToListAsync();
        }
    }
}
