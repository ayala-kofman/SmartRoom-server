using SmartRoom.Core.Enums;

namespace SmartRoom.Core.Entities;

/// <summary>Base class for all rooms. Stored with its subclasses in one table (TPH).</summary>
public abstract class Room
{
    public int Id { get; set; }

    /// <summary>Unique room number, e.g. "204".</summary>
    public string RoomNumber { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public int Floor { get; set; }
    public QualityLevel Quality { get; set; }
    public RoomStatus Status { get; set; } = RoomStatus.Active;

    public ICollection<RoomSlot> Slots { get; set; } = new List<RoomSlot>();
    public ICollection<Feature> Features { get; set; } = new List<Feature>();
}
