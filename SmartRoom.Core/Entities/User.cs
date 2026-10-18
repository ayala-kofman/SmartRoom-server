using SmartRoom.Core.Enums;

namespace SmartRoom.Core.Entities;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;

    /// <summary>Israeli ID number (9 digits). Unique; used for login and never returned in a response DTO.</summary>
    public string IdentityNumber { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    /// <summary>The student's class. Null for teachers and the secretary.</summary>
    public int? ClassGroupId { get; set; }
    public ClassGroup? ClassGroup { get; set; }

    public DateTime CreatedAt { get; set; }

    /// <summary>Classes a teacher teaches (many-to-many, TeacherClassGroups).</summary>
    public ICollection<ClassGroup> TaughtClassGroups { get; set; } = new List<ClassGroup>();

    /// <summary>Bookings made by this user as a teacher.</summary>
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
