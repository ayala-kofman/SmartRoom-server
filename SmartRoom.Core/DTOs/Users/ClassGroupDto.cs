namespace SmartRoom.Core.DTOs.Users;

public class ClassGroupDto
{
    public int Id { get; set; }

    /// <summary>Display name, e.g. "ט'2".</summary>
    public string Name { get; set; } = string.Empty;

    public int Grade { get; set; }
}
