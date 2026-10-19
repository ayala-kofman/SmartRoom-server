using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartRoom.Core.Entities;
using SmartRoom.Core.Enums;

namespace SmartRoom.Data.Configurations;

/// <summary>Regular and computer rooms share one table (TPH) with a RoomType discriminator.</summary>
public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("Rooms");

        builder.HasDiscriminator<RoomType>("RoomType")
            .HasValue<RegularRoom>(RoomType.Regular)
            .HasValue<ComputerRoom>(RoomType.Computer);
        builder.Property<RoomType>("RoomType").HasConversion<string>().HasMaxLength(20);

        builder.Property(r => r.RoomNumber).IsRequired().HasMaxLength(10);
        builder.HasIndex(r => r.RoomNumber).IsUnique();

        builder.Property(r => r.Name).IsRequired().HasMaxLength(100);

        // Quality stays an int, so "best quality first" sorts correctly in SQL.
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);

    }
}

public class RegularRoomConfiguration : IEntityTypeConfiguration<RegularRoom>
{
    public void Configure(EntityTypeBuilder<RegularRoom> builder)
    {
        builder.HasData(
            new RegularRoom { Id = 1, RoomNumber = "101", Name = "כיתה 101", Floor = 1, Quality = QualityLevel.Basic, Status = RoomStatus.Active, SeatCount = 30 },
            new RegularRoom { Id = 2, RoomNumber = "102", Name = "כיתה 102", Floor = 1, Quality = QualityLevel.Standard, Status = RoomStatus.Active, SeatCount = 35 },
            new RegularRoom { Id = 3, RoomNumber = "201", Name = "כיתה 201", Floor = 2, Quality = QualityLevel.Premium, Status = RoomStatus.Active, SeatCount = 40 },
            new RegularRoom { Id = 4, RoomNumber = "202", Name = "כיתה 202", Floor = 2, Quality = QualityLevel.Standard, Status = RoomStatus.UnderMaintenance, SeatCount = 32 },
            new RegularRoom { Id = 5, RoomNumber = "301", Name = "אולם 301", Floor = 3, Quality = QualityLevel.Premium, Status = RoomStatus.Active, SeatCount = 60 });
    }
}

public class ComputerRoomConfiguration : IEntityTypeConfiguration<ComputerRoom>
{
    public void Configure(EntityTypeBuilder<ComputerRoom> builder)
    {
        builder.HasData(
            new ComputerRoom { Id = 6, RoomNumber = "103", Name = "מעבדת מחשבים א", Floor = 1, Quality = QualityLevel.Standard, Status = RoomStatus.Active, ComputerCount = 25 },
            new ComputerRoom { Id = 7, RoomNumber = "203", Name = "מעבדת מחשבים ב", Floor = 2, Quality = QualityLevel.Premium, Status = RoomStatus.Active, ComputerCount = 30 },
            new ComputerRoom { Id = 8, RoomNumber = "302", Name = "מעבדת מחשבים ג", Floor = 3, Quality = QualityLevel.Basic, Status = RoomStatus.Active, ComputerCount = 20 });
    }
}
