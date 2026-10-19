using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartRoom.Core.Entities;

namespace SmartRoom.Data.Configurations;

/// <summary>The limited resource. Version is the concurrency token (see AppDbContext.SaveChanges).</summary>
public class RoomSlotConfiguration : IEntityTypeConfiguration<RoomSlot>
{
    public const string UniqueSlotIndexName = "IX_RoomSlots_RoomId_Date_LessonPeriodId";

    public void Configure(EntityTypeBuilder<RoomSlot> builder)
    {
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);

        builder.Property(s => s.Version).IsConcurrencyToken();

        // One slot per room, date and lesson.
        builder.HasIndex(s => new { s.RoomId, s.Date, s.LessonPeriodId })
            .IsUnique()
            .HasDatabaseName(UniqueSlotIndexName);

        // Search: free slots on a given date.
        builder.HasIndex(s => new { s.Date, s.Status });

        // Deleting a room deletes its slots; the Service blocks it when bookings exist.
        builder.HasOne(s => s.Room)
            .WithMany(r => r.Slots)
            .HasForeignKey(s => s.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.LessonPeriod)
            .WithMany(p => p.Slots)
            .HasForeignKey(s => s.LessonPeriodId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
