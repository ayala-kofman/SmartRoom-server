using System.ComponentModel.DataAnnotations;
using SmartRoom.Core.Enums;

namespace SmartRoom.Core.DTOs.Rooms;

/// <summary>
/// Creates a regular or computer room. The service checks that the field
/// matching RoomType is filled (SeatCount for Regular, ComputerCount for Computer).
/// </summary>
public class CreateRoomRequest
{
    [Required]
    [StringLength(10, MinimumLength = 1)]
    public string RoomNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Range(-2, 20)]
    public int Floor { get; set; }

    [Required]
    [EnumDataType(typeof(QualityLevel))]
    public QualityLevel? Quality { get; set; }

    [Required]
    [EnumDataType(typeof(RoomType))]
    public RoomType? RoomType { get; set; }

    [Range(1, 200)]
    public int? SeatCount { get; set; }

    [Range(1, 200)]
    public int? ComputerCount { get; set; }

    public List<int> FeatureIds { get; set; } = new();
}
