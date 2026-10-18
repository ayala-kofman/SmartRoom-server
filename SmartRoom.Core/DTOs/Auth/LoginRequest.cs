using System.ComponentModel.DataAnnotations;

namespace SmartRoom.Core.DTOs.Auth;

/// <summary>Login is by Israeli ID number only, without a password (see README).</summary>
public class LoginRequest
{
    [Required]
    [RegularExpression(@"^\d{9}$", ErrorMessage = "IdentityNumber must be exactly 9 digits.")]
    public string IdentityNumber { get; set; } = string.Empty;
}
