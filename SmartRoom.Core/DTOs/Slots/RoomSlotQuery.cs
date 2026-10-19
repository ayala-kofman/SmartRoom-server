using System.ComponentModel.DataAnnotations;
using SmartRoom.Core.DTOs.Common;
using SmartRoom.Core.Enums;

namespace SmartRoom.Core.DTOs.Slots;

/// <summary>
/// Filters for GET api/room-slots. Every filter is optional.
/// A teacher always gets available slots only; the service enforces that, not this query.
/// </summary>
public class RoomSlotQuery : PagingQuery
{
    public DateOnly? Date { get; set; }

    [Range(1, int.MaxValue)]
    public int? LessonPeriodId { get; set; }

    [EnumDataType(typeof(RoomType))]
    public RoomType? RoomType { get; set; }

    [EnumDataType(typeof(QualityLevel))]
    public QualityLevel? MinQuality { get; set; }

    /// <summary>Seats for a regular room, computers for a computer room.</summary>
    [Range(1, int.MaxValue)]
    public int? MinCapacity { get; set; }

    [EnumDataType(typeof(SlotStatus))]
    public SlotStatus? Status { get; set; }
}
