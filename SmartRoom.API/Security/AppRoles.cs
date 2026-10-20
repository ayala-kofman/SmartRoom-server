using SmartRoom.Core.Enums;

namespace SmartRoom.API.Security;

/// <summary>
/// Role names for [Authorize(Roles = ...)]. Taken from the UserRole enum, which is also what the token's role claim holds,
/// so a renamed role breaks the build instead of silently locking everyone out.
/// </summary>
public static class AppRoles
{
    public const string Secretary = nameof(UserRole.Secretary);
    public const string Teacher = nameof(UserRole.Teacher);
    public const string Student = nameof(UserRole.Student);

    public const string SecretaryOrTeacher = Secretary + "," + Teacher;
}
