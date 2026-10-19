namespace SmartRoom.Core.Models;

/// <summary>
/// Custom claim names in the JWT. Written by the token generator (Service) and read by the API
/// when it builds CurrentUser, so the name is defined in one place.
/// Id, name and role use the standard ClaimTypes.
/// </summary>
public static class AppClaimTypes
{
    /// <summary>The student's class id. Present only in a student's token.</summary>
    public const string ClassGroupId = "classGroupId";
}
