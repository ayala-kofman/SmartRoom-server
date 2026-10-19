using SmartRoom.Core.DTOs.Rooms;
using SmartRoom.Core.DTOs.Slots;
using SmartRoom.Core.DTOs.Users;
using SmartRoom.Core.Results;

namespace SmartRoom.Core.Interfaces.Services;

public interface IReferenceDataService
{
    Task<Result<IReadOnlyList<ClassGroupDto>>> GetClassGroupsAsync(CancellationToken ct);
    Task<Result<IReadOnlyList<LessonPeriodDto>>> GetLessonPeriodsAsync(CancellationToken ct);
    Task<Result<IReadOnlyList<FeatureDto>>> GetFeaturesAsync(CancellationToken ct);
}
