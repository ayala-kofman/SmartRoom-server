using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartRoom.API.Models;
using SmartRoom.API.Security;
using SmartRoom.Core.DTOs.Bookings;
using SmartRoom.Core.DTOs.Common;
using SmartRoom.Core.Interfaces.Services;

namespace SmartRoom.API.Controllers;

/// <summary>
/// Bookings — the heart of the system. Who sees and manages what comes from the token (CurrentUser),
/// never from the request.
/// </summary>
[Route("api/bookings")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class BookingsController : ApiControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService) => _bookingService = bookingService;

    /// <summary>
    /// Books a slot for a class the teacher teaches. 409 with code SLOT_TAKEN when another teacher took it first.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = AppRoles.Teacher)]
    [ProducesResponseType<BookingDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request, CancellationToken ct) =>
        FromCreated(await _bookingService.CreateAsync(request, CurrentUser, ct), nameof(GetById), booking => new { id = booking.Id });

    /// <summary>
    /// A teacher gets her bookings, a student her class's bookings for one day (today by default), the secretary all.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<BookingDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll([FromQuery] BookingQuery query, CancellationToken ct) =>
        FromResult(await _bookingService.GetPagedAsync(query, CurrentUser, ct));

    /// <summary>The booking's teacher or the secretary.</summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = AppRoles.SecretaryOrTeacher)]
    [ProducesResponseType<BookingDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct) =>
        FromResult(await _bookingService.GetByIdAsync(id, CurrentUser, ct));

    /// <summary>Cancels the booking and frees the slot, until the end of the day before the lesson.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = AppRoles.SecretaryOrTeacher)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel([FromRoute] int id, CancellationToken ct) =>
        FromResult(await _bookingService.CancelAsync(id, CurrentUser, ct));
}
