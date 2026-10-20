using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartRoom.Core.DTOs.Rooms;
using SmartRoom.Core.DTOs.Slots;
using SmartRoom.Core.DTOs.Users;
using SmartRoom.Core.Interfaces.Services;

namespace SmartRoom.API.Controllers;

/// <summary>Fixed lists for the client's dropdowns: classes, lesson periods and room features.</summary>
[Route("api")]
public class ReferenceDataController : ApiControllerBase
{
    private readonly IReferenceDataService _referenceData;

    public ReferenceDataController(IReferenceDataService referenceData) => _referenceData = referenceData;

    /// <summary>Open, because the registration form needs the class list before login.</summary>
    [HttpGet("class-groups")]
    [AllowAnonymous]
    [ProducesResponseType<IReadOnlyList<ClassGroupDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClassGroups(CancellationToken ct) =>
        FromResult(await _referenceData.GetClassGroupsAsync(ct));

    [HttpGet("lesson-periods")]
    [ProducesResponseType<IReadOnlyList<LessonPeriodDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetLessonPeriods(CancellationToken ct) =>
        FromResult(await _referenceData.GetLessonPeriodsAsync(ct));

    [HttpGet("features")]
    [ProducesResponseType<IReadOnlyList<FeatureDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFeatures(CancellationToken ct) =>
        FromResult(await _referenceData.GetFeaturesAsync(ct));
}
