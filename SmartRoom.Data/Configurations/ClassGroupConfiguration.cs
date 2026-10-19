using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartRoom.Core.Entities;

namespace SmartRoom.Data.Configurations;

public class ClassGroupConfiguration : IEntityTypeConfiguration<ClassGroup>
{
    public void Configure(EntityTypeBuilder<ClassGroup> builder)
    {
        builder.Property(c => c.Name).IsRequired().HasMaxLength(20);
        builder.HasIndex(c => c.Name).IsUnique();

        builder.HasData(
            new ClassGroup { Id = 1, Name = "ט'1", Grade = 9 },
            new ClassGroup { Id = 2, Name = "ט'2", Grade = 9 },
            new ClassGroup { Id = 3, Name = "י'1", Grade = 10 },
            new ClassGroup { Id = 4, Name = "י'2", Grade = 10 },
            new ClassGroup { Id = 5, Name = "י\"א1", Grade = 11 },
            new ClassGroup { Id = 6, Name = "י\"ב1", Grade = 12 });
    }
}
