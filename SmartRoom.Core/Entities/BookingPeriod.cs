namespace SmartRoom.Core.Entities;

/// <summary>A date range the secretary opens for booking; opening it creates the room slots.</summary>
public class BookingPeriod
{
    public int Id { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public int CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
