using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartRoom.Core.Entities;

namespace SmartRoom.Data.Configurations;

public class BookingPeriodConfiguration : IEntityTypeConfiguration<BookingPeriod>
{
    public void Configure(EntityTypeBuilder<BookingPeriod> builder)
    {
        builder.ToTable(t => t.HasCheckConstraint("CK_BookingPeriods_Dates", "\"EndDate\" >= \"StartDate\""));

        builder.HasOne(p => p.CreatedByUser)
            .WithMany()
            .HasForeignKey(p => p.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
