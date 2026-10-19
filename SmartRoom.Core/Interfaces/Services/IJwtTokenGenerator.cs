using SmartRoom.Core.Entities;

namespace SmartRoom.Core.Interfaces.Services;

public interface IJwtTokenGenerator
{
    /// <summary>Creates a signed token with user id, name, role and (for a student) class id claims.</summary>
    (string Token, DateTime ExpiresAt) Generate(User user);
}
