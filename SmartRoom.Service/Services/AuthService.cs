using Microsoft.Extensions.Logging;
using SmartRoom.Core.DTOs.Auth;
using SmartRoom.Core.DTOs.Users;
using SmartRoom.Core.Enums;
using SmartRoom.Core.Interfaces.Repositories;
using SmartRoom.Core.Interfaces.Services;
using SmartRoom.Core.Results;

namespace SmartRoom.Service.Services;

/// <summary>Open student registration and login by ID number (no password — see README).</summary>
public class AuthService : IAuthService
{
    // One message for every login failure, so the response does not reveal which ID numbers exist.
    private const string LoginFailedMessage = "Login failed. Check the ID number and try again.";

    private readonly IUserService _userService;
    private readonly IUserRepository _users;
    private readonly IIdentityNumberValidator _identityValidator;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserService userService,
        IUserRepository users,
        IIdentityNumberValidator identityValidator,
        IJwtTokenGenerator tokenGenerator,
        ILogger<AuthService> logger)
    {
        _userService = userService;
        _users = users;
        _identityValidator = identityValidator;
        _tokenGenerator = tokenGenerator;
        _logger = logger;
    }

    public Task<Result<UserDto>> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        // Same checks and save as a student created by the secretary; the role is fixed here, never taken from the client.
        var createRequest = new CreateUserRequest
        {
            FullName = request.FullName,
            IdentityNumber = request.IdentityNumber,
            Role = UserRole.Student,
            ClassGroupId = request.ClassGroupId
        };

        return _userService.CreateAsync(createRequest, ct);
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var user = _identityValidator.IsValid(request.IdentityNumber)
            ? await _users.GetByIdentityNumberAsync(request.IdentityNumber, ct)
            : null;

        if (user is null)
        {
            // The ID number is never logged.
            _logger.LogWarning("Login failed: invalid or unknown ID number.");
            return Result<LoginResponse>.Unauthorized(LoginFailedMessage);
        }

        var (token, expiresAt) = _tokenGenerator.Generate(user);

        _logger.LogInformation("User {UserId} logged in with role {Role}.", user.Id, user.Role);
        return Result<LoginResponse>.Success(new LoginResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            UserId = user.Id,
            FullName = user.FullName,
            Role = user.Role,
            ClassGroupId = user.ClassGroupId,
            TaughtClassGroups = user.TaughtClassGroups
                .OrderBy(c => c.Grade).ThenBy(c => c.Name)
                .Select(c => new ClassGroupDto { Id = c.Id, Name = c.Name, Grade = c.Grade })
                .ToList()
        });
    }
}
