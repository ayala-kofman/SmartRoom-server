using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartRoom.Core.Entities;

namespace SmartRoom.Data.Configurations;

public class LessonPeriodConfiguration : IEntityTypeConfiguration<LessonPeriod>
{
    public void Configure(EntityTypeBuilder<LessonPeriod> builder)
    {
        builder.HasIndex(p => p.LessonNumber).IsUnique();

        builder.HasData(
            new LessonPeriod { Id = 1, LessonNumber = 1, StartTime = new(8, 0), EndTime = new(8, 45) },
            new LessonPeriod { Id = 2, LessonNumber = 2, StartTime = new(8, 45), EndTime = new(9, 30) },
            new LessonPeriod { Id = 3, LessonNumber = 3, StartTime = new(9, 45), EndTime = new(10, 30) },
            new LessonPeriod { Id = 4, LessonNumber = 4, StartTime = new(10, 30), EndTime = new(11, 15) },
            new LessonPeriod { Id = 5, LessonNumber = 5, StartTime = new(11, 30), EndTime = new(12, 15) },
            new LessonPeriod { Id = 6, LessonNumber = 6, StartTime = new(12, 15), EndTime = new(13, 0) },
            new LessonPeriod { Id = 7, LessonNumber = 7, StartTime = new(13, 15), EndTime = new(14, 0) },
            new LessonPeriod { Id = 8, LessonNumber = 8, StartTime = new(14, 0), EndTime = new(14, 45) },
            new LessonPeriod { Id = 9, LessonNumber = 9, StartTime = new(14, 45), EndTime = new(15, 30) },
            new LessonPeriod { Id = 10, LessonNumber = 10, StartTime = new(15, 45), EndTime = new(16, 30) },
            new LessonPeriod { Id = 11, LessonNumber = 11, StartTime = new(16, 30), EndTime = new(17, 15) },
            new LessonPeriod { Id = 12, LessonNumber = 12, StartTime = new(17, 30), EndTime = new(18, 15) });
    }
}
