namespace SmartRoom.Core.Entities;

public class Booking
{
    public int Id { get; set; }

    public int RoomSlotId { get; set; }
    public RoomSlot RoomSlot { get; set; } = null!;

    public int TeacherId { get; set; }
    public User Teacher { get; set; } = null!;

    public int ClassGroupId { get; set; }
    public ClassGroup ClassGroup { get; set; } = null!;

    /// <summary>Optional note or lesson topic.</summary>
    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public bool IsActive { get; set; } = true;
}
