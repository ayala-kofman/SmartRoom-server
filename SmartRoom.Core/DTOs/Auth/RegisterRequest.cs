using System.ComponentModel.DataAnnotations;

namespace SmartRoom.Core.DTOs.Auth;

/// <summary>
/// Open self-registration, for students only (teachers are created by the secretary).
/// The ID check digit is validated in the service; here only the format is checked.
/// </summary>
public class RegisterRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{9}$", ErrorMessage = "IdentityNumber must be exactly 9 digits.")]
    public string IdentityNumber { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "ClassGroupId is required.")]
    public int ClassGroupId { get; set; }
}
