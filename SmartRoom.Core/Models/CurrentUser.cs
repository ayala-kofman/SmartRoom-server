using SmartRoom.Core.Enums;

namespace SmartRoom.Core.Models;

/// <summary>
/// The logged-in user, built by the API from the token claims and passed to the services,
/// so services never depend on HttpContext.
/// </summary>
/// <param name="ClassGroupId">The student's class; null for teachers and the secretary.</param>
public record CurrentUser(int Id, UserRole Role, int? ClassGroupId);
