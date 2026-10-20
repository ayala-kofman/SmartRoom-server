using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartRoom.API.Models;
using SmartRoom.Core.DTOs.Auth;
using SmartRoom.Core.DTOs.Users;
using SmartRoom.Core.Interfaces.Services;

namespace SmartRoom.API.Controllers;

/// <summary>Open endpoints: student self-registration and login by ID number.</summary>
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    /// <summary>Registers a student. Teachers are created by the secretary (POST api/users).</summary>
    [HttpPost("register")]
    [ProducesResponseType<UserDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct) =>
        FromResult(await _authService.RegisterAsync(request, ct));

    /// <summary>Returns a JWT. Paste its token in Swagger's Authorize button.</summary>
    [HttpPost("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct) =>
        FromResult(await _authService.LoginAsync(request, ct));
}
