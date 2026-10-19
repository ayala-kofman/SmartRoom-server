namespace SmartRoom.Core.DTOs.Bookings;

/// <summary>A flat, readable view of a booking: room, date, lesson hours, teacher and class.</summary>
public class BookingDto
{
    public int Id { get; set; }
    public int RoomSlotId { get; set; }

    public string RoomName { get; set; } = string.Empty;
    public string RoomNumber { get; set; } = string.Empty;
    public int Floor { get; set; }

    public DateOnly Date { get; set; }
    public int LessonNumber { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public string TeacherName { get; set; } = string.Empty;
    public string ClassGroupName { get; set; } = string.Empty;
    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CancelledAt { get; set; }
}
