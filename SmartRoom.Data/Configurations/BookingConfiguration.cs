using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartRoom.Core.Entities;

namespace SmartRoom.Data.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public const string ActiveBookingIndexName = "IX_Bookings_RoomSlotId_Active";

    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.Property(b => b.Note).HasMaxLength(200);

        // Second line of defence behind the Version token: one active booking per slot.
        builder.HasIndex(b => b.RoomSlotId)
            .IsUnique()
            .HasFilter("\"IsActive\" = TRUE")
            .HasDatabaseName(ActiveBookingIndexName);

        builder.HasIndex(b => b.TeacherId);
        builder.HasIndex(b => b.ClassGroupId);

        // Booking history must survive; nothing here deletes bookings.
        builder.HasOne(b => b.RoomSlot)
            .WithMany(s => s.Bookings)
            .HasForeignKey(b => b.RoomSlotId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Teacher)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.ClassGroup)
            .WithMany(c => c.Bookings)
            .HasForeignKey(b => b.ClassGroupId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
