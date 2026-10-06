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
    }
}
