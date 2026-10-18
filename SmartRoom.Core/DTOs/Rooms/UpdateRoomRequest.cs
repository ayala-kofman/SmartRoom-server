using System.ComponentModel.DataAnnotations;
using SmartRoom.Core.Enums;

namespace SmartRoom.Core.DTOs.Rooms;

/// <summary>
/// Full update of a room. The room type cannot change (it is the TPH discriminator);
/// only the capacity field of the existing type is used.
/// </summary>
public class UpdateRoomRequest
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

    /// <summary>Closing a room here is how the secretary "removes" a room that has bookings.</summary>
    [Required]
    [EnumDataType(typeof(RoomStatus))]
    public RoomStatus? Status { get; set; }

    [Range(1, 200)]
    public int? SeatCount { get; set; }

    [Range(1, 200)]
    public int? ComputerCount { get; set; }

    public List<int> FeatureIds { get; set; } = new();
}
