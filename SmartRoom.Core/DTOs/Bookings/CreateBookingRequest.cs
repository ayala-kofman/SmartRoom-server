using System.ComponentModel.DataAnnotations;

namespace SmartRoom.Core.DTOs.Bookings;

/// <summary>Body of POST api/bookings. The teacher id comes from the token, not from the client.</summary>
public class CreateBookingRequest
{
    [Required]
    [Range(1, int.MaxValue)]
    public int? RoomSlotId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int? ClassGroupId { get; set; }

    /// <summary>Optional note or lesson topic.</summary>
    [MaxLength(200)]
    public string? Note { get; set; }
}
