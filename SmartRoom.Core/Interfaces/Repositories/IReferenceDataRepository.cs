using SmartRoom.Core.Entities;

namespace SmartRoom.Core.Interfaces.Repositories;

/// <summary>Read access to the fixed lookup tables: class groups, lesson periods and features.</summary>
public interface IReferenceDataRepository
{
    Task<IReadOnlyList<ClassGroup>> GetClassGroupsAsync(CancellationToken ct);
    Task<IReadOnlyList<LessonPeriod>> GetLessonPeriodsAsync(CancellationToken ct);
    Task<IReadOnlyList<Feature>> GetFeaturesAsync(CancellationToken ct);

    /// <summary>TRACKED class groups by id, attached to a new teacher. Missing ids are simply not returned.</summary>
    Task<IReadOnlyList<ClassGroup>> GetClassGroupsByIdsAsync(IEnumerable<int> ids, CancellationToken ct);

    /// <summary>TRACKED features by id, attached to a new or updated room. Missing ids are simply not returned.</summary>
    Task<IReadOnlyList<Feature>> GetFeaturesByIdsAsync(IEnumerable<int> ids, CancellationToken ct);

    Task<bool> ClassGroupExistsAsync(int id, CancellationToken ct);
}
