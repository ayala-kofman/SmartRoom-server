using SmartRoom.Core.Enums;

namespace SmartRoom.Core.DTOs.Slots;

/// <summary>A flat view of one slot: the room, the date and the lesson hour.</summary>
public class RoomSlotDto
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public SlotStatus Status { get; set; }

    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public int Floor { get; set; }
    public RoomType RoomType { get; set; }
    public QualityLevel Quality { get; set; }

    /// <summary>Seats for a regular room, computers for a computer room.</summary>
    public int Capacity { get; set; }

    public int LessonNumber { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}
