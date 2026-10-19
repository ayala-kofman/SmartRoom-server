using SmartRoom.Core.DTOs.Auth;
using SmartRoom.Core.DTOs.Users;
using SmartRoom.Core.Results;

namespace SmartRoom.Core.Interfaces.Services;

public interface IAuthService
{
    /// <summary>Student self-registration. Created, ValidationError (bad ID / class), or Conflict (ID exists).</summary>
    Task<Result<UserDto>> RegisterAsync(RegisterRequest request, CancellationToken ct);

    /// <summary>Success with a token, or Unauthorized with a general message.</summary>
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct);
}
