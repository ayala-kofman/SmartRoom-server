using AutoMapper;
using SmartRoom.Core.DTOs.Rooms;
using SmartRoom.Core.DTOs.Slots;
using SmartRoom.Core.DTOs.Users;
using SmartRoom.Core.Interfaces.Repositories;
using SmartRoom.Core.Interfaces.Services;
using SmartRoom.Core.Results;

namespace SmartRoom.Service.Services;

public class ReferenceDataService : IReferenceDataService
{
    private readonly IReferenceDataRepository _referenceData;
    private readonly IMapper _mapper;

    public ReferenceDataService(IReferenceDataRepository referenceData, IMapper mapper)
    {
        _referenceData = referenceData;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<ClassGroupDto>>> GetClassGroupsAsync(CancellationToken ct) =>
        Result<IReadOnlyList<ClassGroupDto>>.Success(
            _mapper.Map<List<ClassGroupDto>>(await _referenceData.GetClassGroupsAsync(ct)));

    public async Task<Result<IReadOnlyList<LessonPeriodDto>>> GetLessonPeriodsAsync(CancellationToken ct) =>
        Result<IReadOnlyList<LessonPeriodDto>>.Success(
            _mapper.Map<List<LessonPeriodDto>>(await _referenceData.GetLessonPeriodsAsync(ct)));

    public async Task<Result<IReadOnlyList<FeatureDto>>> GetFeaturesAsync(CancellationToken ct) =>
        Result<IReadOnlyList<FeatureDto>>.Success(
            _mapper.Map<List<FeatureDto>>(await _referenceData.GetFeaturesAsync(ct)));
}
