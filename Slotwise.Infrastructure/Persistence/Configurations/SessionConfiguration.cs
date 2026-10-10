using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Slotwise.Domain.Sessions.Entities;

namespace Slotwise.Infrastructure.Persistence.Configurations
{
    public class SessionConfiguration : IEntityTypeConfiguration<Session>
    {
        public void Configure(EntityTypeBuilder<Session> builder)
        {
            builder.ToTable("Sessions");

            builder.HasKey(s => s.Id);
            builder.Property(s => s.Id).ValueGeneratedNever();

            builder.Property(s => s.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.OwnsOne(s => s.TimeSlot, timeSlot =>
            {
                timeSlot.Property(t => t.Start).HasColumnName("StartsAt").IsRequired();
                timeSlot.Property(t => t.End).HasColumnName("EndsAt").IsRequired();
            });
            builder.Navigation(s => s.TimeSlot).IsRequired();

            builder.OwnsOne(s => s.SeatCount, seatCount =>
            {
                seatCount.Property(c => c.Value).HasColumnName("SeatCount").IsRequired();
            });
            builder.Navigation(s => s.SeatCount).IsRequired();

            builder.HasMany(s => s.Bookings)
                .WithOne()
                .HasForeignKey(b => b.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(s => s.Bookings)
                .HasField("bookings")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Ignore(s => s.DomainEvents);
        }
    }
}
