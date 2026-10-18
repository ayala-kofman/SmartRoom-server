using SmartRoom.Core.Enums;

namespace SmartRoom.Core.DTOs.Rooms;

public class RoomDto
{
    public int Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Floor { get; set; }
    public QualityLevel Quality { get; set; }
    public RoomStatus Status { get; set; }
    public RoomType RoomType { get; set; }

    /// <summary>Seats for a regular room, computers for a computer room.</summary>
    public int Capacity { get; set; }

    public List<string> Features { get; set; } = new();
}
