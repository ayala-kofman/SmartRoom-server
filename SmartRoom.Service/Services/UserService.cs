using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartRoom.Core.DTOs.Common;
using SmartRoom.Core.DTOs.Users;
using SmartRoom.Core.Entities;
using SmartRoom.Core.Enums;
using SmartRoom.Core.Exceptions;
using SmartRoom.Core.Interfaces.Repositories;
using SmartRoom.Core.Interfaces.Services;
using SmartRoom.Core.Options;
using SmartRoom.Core.Results;

namespace SmartRoom.Service.Services;

/// <summary>The secretary's user management: creating teachers and students, listing users.</summary>
public class UserService : IUserService
{
    private const string IdentityNumberTakenMessage = "A user with this ID number already exists.";

    private readonly IUserRepository _users;
    private readonly IReferenceDataRepository _referenceData;
    private readonly IIdentityNumberValidator _identityValidator;
    private readonly TimeProvider _time;
    private readonly IMapper _mapper;
    private readonly PagingOptions _paging;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository users,
        IReferenceDataRepository referenceData,
        IIdentityNumberValidator identityValidator,
        TimeProvider time,
        IMapper mapper,
        IOptions<PagingOptions> paging,
        ILogger<UserService> logger)
    {
        _users = users;
        _referenceData = referenceData;
        _identityValidator = identityValidator;
        _time = time;
        _mapper = mapper;
        _paging = paging.Value;
        _logger = logger;
    }

    public async Task<Result<UserDto>> CreateAsync(CreateUserRequest request, CancellationToken ct)
    {
        // A secretary account exists only from the seed; no request may create one.
        if (request.Role is not (UserRole.Teacher or UserRole.Student))
            return Result<UserDto>.ValidationError("Only a teacher or a student can be created.");

        if (!_identityValidator.IsValid(request.IdentityNumber))
            return Result<UserDto>.ValidationError("IdentityNumber is not a valid ID number.");

        var classIds = request.Role == UserRole.Teacher
            ? request.ClassGroupIds.Distinct().ToList()
            : new List<int>();

        var fieldsError = CheckRoleFields(request, classIds);
        if (fieldsError is not null)
            return Result<UserDto>.ValidationError(fieldsError);

        if (request.Role == UserRole.Student)
            classIds.Add(request.ClassGroupId!.Value);

        var classes = await _referenceData.GetClassGroupsByIdsAsync(classIds, ct);
        if (classes.Count != classIds.Count)
            return Result<UserDto>.ValidationError("One or more class group ids do not exist.");

        if (await _users.IdentityNumberExistsAsync(request.IdentityNumber, ct))
            return Result<UserDto>.Conflict(IdentityNumberTakenMessage, ErrorCodes.IdentityNumberTaken);

        var user = _mapper.Map<User>(request);
        user.CreatedAt = _time.GetUtcNow().UtcDateTime;
        if (user.Role == UserRole.Teacher)
        {
            user.ClassGroupId = null;
            user.TaughtClassGroups = classes.ToList();
        }
        else
        {
            // Navigation set from the tracked class so the DTO gets the class name without reloading.
            user.ClassGroup = classes[0];
        }

        await _users.AddAsync(user, ct);
        try
        {
            await _users.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException)
        {
            // Another request saved the same ID number between the check and the save (unique index).
            return Result<UserDto>.Conflict(IdentityNumberTakenMessage, ErrorCodes.IdentityNumberTaken);
        }

        // The ID number is never logged; the user id is enough.
        _logger.LogInformation("User {UserId} created with role {Role}.", user.Id, user.Role);
        return Result<UserDto>.Created(_mapper.Map<UserDto>(user));
    }

    public async Task<Result<PagedResult<UserDto>>> GetPagedAsync(UserQuery query, CancellationToken ct)
    {
        var page = await _users.GetPagedAsync(query, _paging.ResolvePageSize(query.PageSize), ct);
        return Result<PagedResult<UserDto>>.Success(_mapper.MapPage<User, UserDto>(page));
    }

    /// <summary>A student has exactly one class (ClassGroupId); a teacher has at least one taught class (ClassGroupIds).</summary>
    private static string? CheckRoleFields(CreateUserRequest request, List<int> teacherClassIds) => request.Role switch
    {
        UserRole.Student when request.ClassGroupId is null => "ClassGroupId is required for a student.",
        UserRole.Student when request.ClassGroupIds.Count > 0 => "ClassGroupIds is for teachers only; use ClassGroupId for a student.",
        UserRole.Teacher when teacherClassIds.Count == 0 => "A teacher needs at least one class in ClassGroupIds.",
        UserRole.Teacher when request.ClassGroupId is not null => "ClassGroupId is for students only; use ClassGroupIds for a teacher.",
        UserRole.Teacher when teacherClassIds.Any(id => id <= 0) => "ClassGroupIds must contain positive ids.",
        _ => null
    };
}
