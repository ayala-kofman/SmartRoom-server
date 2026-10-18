using SmartRoom.Core.Enums;

namespace SmartRoom.Core.DTOs.Auth;

/// <summary>Returned after a successful login. Never includes the ID number.</summary>
public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }

    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }

    /// <summary>The student's class; null for teachers and the secretary.</summary>
    public int? ClassGroupId { get; set; }
}
