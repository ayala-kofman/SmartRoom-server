using Microsoft.EntityFrameworkCore;
using SmartRoom.Core.Entities;
using SmartRoom.Core.Interfaces.Repositories;

namespace SmartRoom.Data.Repositories;

public class ReferenceDataRepository : IReferenceDataRepository
{
    private readonly AppDbContext _db;

    public ReferenceDataRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ClassGroup>> GetClassGroupsAsync(CancellationToken ct) =>
        await _db.ClassGroups.AsNoTracking().OrderBy(c => c.Grade).ThenBy(c => c.Name).ToListAsync(ct);

    public async Task<IReadOnlyList<LessonPeriod>> GetLessonPeriodsAsync(CancellationToken ct) =>
        await _db.LessonPeriods.AsNoTracking().OrderBy(p => p.LessonNumber).ToListAsync(ct);

    public async Task<IReadOnlyList<Feature>> GetFeaturesAsync(CancellationToken ct) =>
        await _db.Features.AsNoTracking().OrderBy(f => f.Name).ToListAsync(ct);

    public async Task<IReadOnlyList<ClassGroup>> GetClassGroupsByIdsAsync(IEnumerable<int> ids, CancellationToken ct)
    {
        var idList = ids.Distinct().ToList();
        return await _db.ClassGroups.Where(c => idList.Contains(c.Id)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Feature>> GetFeaturesByIdsAsync(IEnumerable<int> ids, CancellationToken ct)
    {
        var idList = ids.Distinct().ToList();
        return await _db.Features.Where(f => idList.Contains(f.Id)).ToListAsync(ct);
    }

    public Task<bool> ClassGroupExistsAsync(int id, CancellationToken ct) =>
        _db.ClassGroups.AnyAsync(c => c.Id == id, ct);
}
