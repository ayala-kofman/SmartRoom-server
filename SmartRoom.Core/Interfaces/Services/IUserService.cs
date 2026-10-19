using SmartRoom.Core.DTOs.Common;
using SmartRoom.Core.DTOs.Users;
using SmartRoom.Core.Results;

namespace SmartRoom.Core.Interfaces.Services;

public interface IUserService
{
    /// <summary>
    /// The secretary creates a teacher (with classes) or a student (with a class).
    /// ValidationError for Role = Secretary or missing role fields; Conflict if the ID exists.
    /// </summary>
    Task<Result<UserDto>> CreateAsync(CreateUserRequest request, CancellationToken ct);

    Task<Result<PagedResult<UserDto>>> GetPagedAsync(UserQuery query, CancellationToken ct);
}
