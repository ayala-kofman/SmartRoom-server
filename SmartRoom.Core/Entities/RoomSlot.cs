using System.ComponentModel.DataAnnotations;
using SmartRoom.Core.Enums;

namespace SmartRoom.Core.Entities;

/// <summary>
/// The limited resource: one room, on one date, in one lesson period.
/// Booking it is an UPDATE of this row, so the concurrency token catches simultaneous bookings.
/// </summary>
public class RoomSlot
{
    public int Id { get; set; }

    public int RoomId { get; set; }
    public Room Room { get; set; } = null!;

    /// <summary>Lesson date only (no time of day).</summary>
    public DateOnly Date { get; set; }

    public int LessonPeriodId { get; set; }
    public LessonPeriod LessonPeriod { get; set; } = null!;

    public SlotStatus Status { get; set; } = SlotStatus.Available;

    /// <summary>
    /// Concurrency token. EF adds "WHERE Version = @original" to every UPDATE;
    /// AppDbContext replaces it with a new Guid on each save of a modified slot.
    /// </summary>
    [ConcurrencyCheck]
    public Guid Version { get; set; } = Guid.NewGuid();

    /// <summary>Booking history for this slot; at most one is active.</summary>
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
