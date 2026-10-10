using Microsoft.EntityFrameworkCore;
using Slotwise.Application.Sessions.Interfaces;
using Slotwise.Domain.Sessions.Entities;

namespace Slotwise.Infrastructure.Persistence.Repositories
{
    public class SessionRepository : ISessionRepository
    {
        private readonly SlotwiseDbContext _context;

        public SessionRepository(SlotwiseDbContext context)
        {
            _context = context;
        }

        public void Add(Session session)
        {
            _context.Sessions.Add(session);
        }

        public async Task<Session?> GetByIdAsync(Guid id)
        {
            return await _context.Sessions.Include(s => s.Bookings).FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
