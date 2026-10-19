using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartRoom.Core.Entities;

namespace SmartRoom.Data.Configurations;

public class FeatureConfiguration : IEntityTypeConfiguration<Feature>
{
    public void Configure(EntityTypeBuilder<Feature> builder)
    {
        builder.Property(f => f.Name).IsRequired().HasMaxLength(50);
        builder.HasIndex(f => f.Name).IsUnique();

        builder.HasData(
            new Feature { Id = 1, Name = "מקרן" },
            new Feature { Id = 2, Name = "לוח חכם" },
            new Feature { Id = 3, Name = "מזגן" },
            new Feature { Id = 4, Name = "מערכת הגברה" });

        // Room ↔ Feature (many-to-many). Deleting a room or a feature removes only the link rows.
        builder.HasMany(f => f.Rooms)
            .WithMany(r => r.Features)
            .UsingEntity(
                "RoomFeatures",
                r => r.HasOne(typeof(Room)).WithMany().HasForeignKey("RoomId"),
                l => l.HasOne(typeof(Feature)).WithMany().HasForeignKey("FeatureId"),
                j =>
                {
                    j.HasKey("RoomId", "FeatureId");
                    j.HasData(
                        new { RoomId = 1, FeatureId = 3 },
                        new { RoomId = 2, FeatureId = 1 },
                        new { RoomId = 2, FeatureId = 3 },
                        new { RoomId = 3, FeatureId = 1 },
                        new { RoomId = 3, FeatureId = 2 },
                        new { RoomId = 3, FeatureId = 3 },
                        new { RoomId = 4, FeatureId = 1 },
                        new { RoomId = 5, FeatureId = 1 },
                        new { RoomId = 5, FeatureId = 3 },
                        new { RoomId = 5, FeatureId = 4 },
                        new { RoomId = 6, FeatureId = 1 },
                        new { RoomId = 6, FeatureId = 3 },
                        new { RoomId = 7, FeatureId = 1 },
                        new { RoomId = 7, FeatureId = 2 },
                        new { RoomId = 7, FeatureId = 3 },
                        new { RoomId = 8, FeatureId = 3 });
                });
    }
}
