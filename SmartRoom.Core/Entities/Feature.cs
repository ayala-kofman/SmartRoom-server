namespace SmartRoom.Core.Entities;

/// <summary>Fixed equipment in a room (projector, smart board, air conditioner...).</summary>
public class Feature
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}
