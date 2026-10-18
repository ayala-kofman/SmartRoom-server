namespace SmartRoom.Core.Entities;

public class ClassGroup
{
    public int Id { get; set; }

    /// <summary>Display name, e.g. "ט'2".</summary>
    public string Name { get; set; } = string.Empty;

    public int Grade { get; set; }

    public ICollection<User> Students { get; set; } = new List<User>();
    public ICollection<User> Teachers { get; set; } = new List<User>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
