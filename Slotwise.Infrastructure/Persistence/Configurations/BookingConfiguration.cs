using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Slotwise.Domain.Sessions.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Slotwise.Infrastructure.Persistence.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.ToTable("Booking");
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id).IsRequired().ValueGeneratedNever();
            builder.Property(b => b.SessionId).IsRequired().ValueGeneratedNever();
            builder.OwnsOne(b => b.EmailAddress, email =>
            {
                email.Property(e => e.Value).HasColumnName("EmailAddress").HasMaxLength(200).IsRequired();
            });
            builder.Navigation(b => b.EmailAddress).IsRequired();
            builder.Property(b => b.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(b => b.CreatedAt).HasColumnType("datetime2").IsRequired();

        }
    }
}
