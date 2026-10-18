namespace SmartRoom.Core.Entities;

/// <summary>A fixed lesson hour in the school timetable (lesson 1, 2, ...).</summary>
public class LessonPeriod
{
    public int Id { get; set; }
    public int LessonNumber { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public ICollection<RoomSlot> Slots { get; set; } = new List<RoomSlot>();
}
