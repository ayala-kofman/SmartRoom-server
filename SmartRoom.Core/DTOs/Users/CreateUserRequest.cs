using System.ComponentModel.DataAnnotations;
using SmartRoom.Core.Enums;

namespace SmartRoom.Core.DTOs.Users;

/// <summary>
/// Body of POST api/users: the secretary creates a teacher or a student.
/// The service checks the fields that match the role:
/// a student needs ClassGroupId, a teacher needs at least one ClassGroupIds entry,
/// and Role = Secretary is rejected (400).
/// The ID check digit is validated in the service; here only the format is checked.
/// </summary>
public class CreateUserRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{9}$", ErrorMessage = "IdentityNumber must be exactly 9 digits.")]
    public string IdentityNumber { get; set; } = string.Empty;

    [Required]
    [EnumDataType(typeof(UserRole))]
    public UserRole? Role { get; set; }

    /// <summary>Student only: the student's class.</summary>
    [Range(1, int.MaxValue)]
    public int? ClassGroupId { get; set; }

    /// <summary>Teacher only: the classes she teaches.</summary>
    public List<int> ClassGroupIds { get; set; } = new();
}
