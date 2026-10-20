using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartRoom.API.Models;
using SmartRoom.API.Security;
using SmartRoom.Core.DTOs.Slots;
using SmartRoom.Core.Interfaces.Services;

namespace SmartRoom.API.Controllers;

/// <summary>Booking periods: opening one creates the slots of all active rooms for its school days.</summary>
[Route("api/booking-periods")]
[Authorize(Roles = AppRoles.Secretary)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class BookingPeriodsController : ApiControllerBase
{
    private readonly IBookingPeriodService _periodService;

    public BookingPeriodsController(IBookingPeriodService periodService) => _periodService = periodService;

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<BookingPeriodDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        FromResult(await _periodService.GetAllAsync(ct));

    /// <summary>Opens a period. There is no GET by id, so Location points to the list.</summary>
    [HttpPost]
    [ProducesResponseType<BookingPeriodDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateBookingPeriodRequest request, CancellationToken ct) =>
        FromCreated(await _periodService.CreateAsync(request, CurrentUser, ct), nameof(GetAll), _ => new { });
}
