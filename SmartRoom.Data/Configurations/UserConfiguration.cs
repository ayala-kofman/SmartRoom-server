using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartRoom.Core.Entities;

namespace SmartRoom.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <summary>EF's default name, kept explicit so a race on the same ID number can be recognized.</summary>
    public const string UniqueIdentityNumberIndexName = "IX_Users_IdentityNumber";

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.FullName).IsRequired().HasMaxLength(100);

        builder.Property(u => u.IdentityNumber).IsRequired().HasMaxLength(9);
        builder.HasIndex(u => u.IdentityNumber)
            .IsUnique()
            .HasDatabaseName(UniqueIdentityNumberIndexName);

        builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);

        // Student → her class. Deleting a class must not delete students.
        builder.HasOne(u => u.ClassGroup)
            .WithMany(c => c.Students)
            .HasForeignKey(u => u.ClassGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        // Teacher ↔ classes she teaches; used for the booking permission check.
        builder.HasMany(u => u.TaughtClassGroups)
            .WithMany(c => c.Teachers)
            .UsingEntity(
                "TeacherClassGroups",
                r => r.HasOne(typeof(ClassGroup)).WithMany().HasForeignKey("ClassGroupId"),
                l => l.HasOne(typeof(User)).WithMany().HasForeignKey("TeacherId"),
                j => j.HasKey("TeacherId", "ClassGroupId"));
    }
}
