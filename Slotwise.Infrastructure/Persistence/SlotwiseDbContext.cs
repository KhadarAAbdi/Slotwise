using Microsoft.EntityFrameworkCore;
using Slotwise.Domain.Sessions.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slotwise.Infrastructure.Persistence
{
    public class SlotwiseDbContext : DbContext
    {
        public DbSet<Session> Sessions { get; set; }
        public SlotwiseDbContext(DbContextOptions<SlotwiseDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SlotwiseDbContext).Assembly);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            BumpVersionOfChangedSessions();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            BumpVersionOfChangedSessions();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>
        /// A session is the consistency boundary for its bookings: the capacity rule is checked in memory,
        /// so two requests can each see one free seat. Adding a booking only inserts a Booking row and never
        /// touches the Session row, so the concurrency token on Session would not be checked. Bumping the
        /// version whenever one of its bookings changes forces an UPDATE ... WHERE Version = @original, and
        /// the request that loaded a stale session gets a DbUpdateConcurrencyException.
        /// </summary>
        private void BumpVersionOfChangedSessions()
        {
            ChangeTracker.DetectChanges();

            var sessionsWithChangedBookings = ChangeTracker.Entries<Booking>()
                .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                .Select(e => e.Entity.SessionId)
                .ToHashSet();

            foreach (var entry in ChangeTracker.Entries<Session>())
            {
                var isExisting = entry.State is EntityState.Unchanged or EntityState.Modified;

                if (isExisting && sessionsWithChangedBookings.Contains(entry.Entity.Id))
                {
                    var version = entry.Property<int>("Version");
                    version.CurrentValue = version.OriginalValue + 1;
                }
            }
        }
    }
}
