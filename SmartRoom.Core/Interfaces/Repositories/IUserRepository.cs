using SmartRoom.Core.DTOs.Common;
using SmartRoom.Core.DTOs.Users;
using SmartRoom.Core.Entities;

namespace SmartRoom.Core.Interfaces.Repositories;

public interface IUserRepository
{
    /// <summary>For login: the user with this ID number, with class and taught classes. Null if not found.</summary>
    Task<User?> GetByIdentityNumberAsync(string identityNumber, CancellationToken ct);

    Task<bool> IdentityNumberExistsAsync(string identityNumber, CancellationToken ct);

    /// <summary>Filtered page of users with class and taught classes (no tracking).</summary>
    Task<PagedResult<User>> GetPagedAsync(UserQuery query, int pageSize, CancellationToken ct);

    /// <summary>True if the teacher is assigned to teach this class.</summary>
    Task<bool> TeacherTeachesClassAsync(int teacherId, int classGroupId, CancellationToken ct);

    Task AddAsync(User user, CancellationToken ct);

    /// <summary>Saves all pending changes. Throws ConcurrencyConflictException on a concurrency race.</summary>
    Task SaveChangesAsync(CancellationToken ct);
}
