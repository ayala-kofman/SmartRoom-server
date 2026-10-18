using System.ComponentModel.DataAnnotations;
using SmartRoom.Core.DTOs.Common;
using SmartRoom.Core.Enums;

namespace SmartRoom.Core.DTOs.Rooms;

/// <summary>Filters for GET api/rooms. Every filter is optional.</summary>
public class RoomQuery : PagingQuery
{
    [EnumDataType(typeof(RoomType))]
    public RoomType? RoomType { get; set; }

    public int? Floor { get; set; }

    [EnumDataType(typeof(QualityLevel))]
    public QualityLevel? MinQuality { get; set; }

    /// <summary>Seats for a regular room, computers for a computer room.</summary>
    [Range(1, int.MaxValue)]
    public int? MinCapacity { get; set; }

    [EnumDataType(typeof(RoomStatus))]
    public RoomStatus? Status { get; set; }

    /// <summary>Only rooms that have all of these features (?featureIds=1&amp;featureIds=3).</summary>
    public List<int> FeatureIds { get; set; } = new();

    /// <summary>Default: best quality first ("early bird" principle).</summary>
    [EnumDataType(typeof(RoomSortBy))]
    public RoomSortBy SortBy { get; set; } = RoomSortBy.Quality;

    public bool Descending { get; set; } = true;
}
