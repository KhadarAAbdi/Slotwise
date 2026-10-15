using Microsoft.EntityFrameworkCore;
using Slotwise.Application.Abstractions;
using Slotwise.Application.Sessions.Exceptions;
using Slotwise.Application.Sessions.Interfaces;
using Slotwise.Domain.Base;
using Slotwise.Domain.Sessions.Entities;

namespace Slotwise.Infrastructure.Persistence.Repositories
{
    public class SessionRepository : ISessionRepository
    {
        private readonly SlotwiseDbContext _context;
        private readonly IDomainEventDispatcher _dispatcher;

        public SessionRepository(SlotwiseDbContext context, IDomainEventDispatcher dispatcher)
        {
            _context = context;
            _dispatcher = dispatcher;
        }

        public void Add(Session session)
        {
            _context.Sessions.Add(session);
        }

        public async Task<Session?> GetByIdAsync(Guid id)
        {
            return await _context.Sessions
                .Include(s => s.Bookings)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        /// <summary>
        /// Saves, then publishes the events the saved aggregates raised. The order matters:
        /// collect before the save (the tracker still holds the aggregates), dispatch only after it succeeds
        /// (nobody should hear about a change that never happened), clear last (so a later save doesn't redeliver).
        /// </summary>
        public async Task SaveChangesAsync()
        {
            var aggregates = _context.ChangeTracker.Entries<AggregateRoot>()
                .Select(entry => entry.Entity)
                .Where(aggregate => aggregate.DomainEvents.Count > 0)
                .ToList();

            var domainEvents = aggregates.SelectMany(aggregate => aggregate.DomainEvents).ToList();

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new SessionConcurrencyException(ex);
            }

            await _dispatcher.DispatchAsync(domainEvents);

            foreach (var aggregate in aggregates)
            {
                aggregate.ClearDomainEvents();
            }
        }
    }
}
