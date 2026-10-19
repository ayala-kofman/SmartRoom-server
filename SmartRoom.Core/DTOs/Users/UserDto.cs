using SmartRoom.Core.Enums;

namespace SmartRoom.Core.DTOs.Users;

/// <summary>A user as returned by the API. The ID number is never included.</summary>
public class UserDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }

    /// <summary>The student's class. Null for teachers and the secretary.</summary>
    public string? ClassGroupName { get; set; }

    /// <summary>Classes a teacher teaches. Empty for students and the secretary.</summary>
    public List<string> TaughtClassGroups { get; set; } = new();

    public DateTime CreatedAt { get; set; }
}
