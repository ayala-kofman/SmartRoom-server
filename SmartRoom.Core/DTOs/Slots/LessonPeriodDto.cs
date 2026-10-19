namespace SmartRoom.Core.DTOs.Slots;

public class LessonPeriodDto
{
    public int Id { get; set; }
    public int LessonNumber { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}
