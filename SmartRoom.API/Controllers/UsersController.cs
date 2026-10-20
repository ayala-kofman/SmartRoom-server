using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartRoom.API.Models;
using SmartRoom.API.Security;
using SmartRoom.Core.DTOs.Common;
using SmartRoom.Core.DTOs.Users;
using SmartRoom.Core.Interfaces.Services;

namespace SmartRoom.API.Controllers;

/// <summary>User management by the secretary.</summary>
[Route("api/users")]
[Authorize(Roles = AppRoles.Secretary)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class UsersController : ApiControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService) => _userService = userService;

    /// <summary>Creates a teacher (with ClassGroupIds) or a student (with ClassGroupId).</summary>
    [HttpPost]
    [ProducesResponseType<UserDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken ct) =>
        FromResult(await _userService.CreateAsync(request, ct));

    [HttpGet]
    [ProducesResponseType<PagedResult<UserDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] UserQuery query, CancellationToken ct) =>
        FromResult(await _userService.GetPagedAsync(query, ct));
}
