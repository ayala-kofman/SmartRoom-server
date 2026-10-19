using Microsoft.EntityFrameworkCore;
using SmartRoom.Core.DTOs.Common;
using SmartRoom.Core.DTOs.Users;
using SmartRoom.Core.Entities;
using SmartRoom.Core.Enums;
using SmartRoom.Core.Interfaces.Repositories;

namespace SmartRoom.Data.Repositories;

public class UserRepository : RepositoryBase, IUserRepository
{
    public UserRepository(AppDbContext db)
        : base(db)
    {
    }

    public Task<User?> GetByIdentityNumberAsync(string identityNumber, CancellationToken ct) =>
        Db.Users
            .AsNoTracking()
            .Include(u => u.ClassGroup)
            .Include(u => u.TaughtClassGroups)
            .FirstOrDefaultAsync(u => u.IdentityNumber == identityNumber, ct);

    public Task<bool> IdentityNumberExistsAsync(string identityNumber, CancellationToken ct) =>
        Db.Users.AnyAsync(u => u.IdentityNumber == identityNumber, ct);

    public async Task<PagedResult<User>> GetPagedAsync(UserQuery query, int pageSize, CancellationToken ct)
    {
        IQueryable<User> users = Db.Users
            .AsNoTracking()
            .Include(u => u.ClassGroup)
            .Include(u => u.TaughtClassGroups);

        if (query.Role is UserRole role)
            users = users.Where(u => u.Role == role);

        users = users.OrderBy(u => u.FullName).ThenBy(u => u.Id);

        return await users.ToPagedResultAsync(query.Page, pageSize, ct);
    }

    public Task<bool> TeacherTeachesClassAsync(int teacherId, int classGroupId, CancellationToken ct) =>
        Db.Users.AnyAsync(u =>
            u.Id == teacherId
            && u.Role == UserRole.Teacher
            && u.TaughtClassGroups.Any(c => c.Id == classGroupId), ct);

    public async Task AddAsync(User user, CancellationToken ct) => await Db.Users.AddAsync(user, ct);
}
