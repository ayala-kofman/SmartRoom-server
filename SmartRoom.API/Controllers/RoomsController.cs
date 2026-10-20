using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartRoom.API.Models;
using SmartRoom.API.Security;
using SmartRoom.Core.DTOs.Common;
using SmartRoom.Core.DTOs.Rooms;
using SmartRoom.Core.Interfaces.Services;

namespace SmartRoom.API.Controllers;

/// <summary>Rooms: the secretary manages them, teachers browse them.</summary>
[Route("api/rooms")]
[Authorize(Roles = AppRoles.SecretaryOrTeacher)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class RoomsController : ApiControllerBase
{
    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService) => _roomService = roomService;

    /// <summary>Filtered, sorted and paged list of rooms.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<RoomDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] RoomQuery query, CancellationToken ct) =>
        FromResult(await _roomService.GetPagedAsync(query, ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType<RoomDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct) =>
        FromResult(await _roomService.GetByIdAsync(id, ct));

    /// <summary>Creates a room, and its slots for every open booking period.</summary>
    [HttpPost]
    [Authorize(Roles = AppRoles.Secretary)]
    [ProducesResponseType<RoomDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateRoomRequest request, CancellationToken ct) =>
        FromCreated(await _roomService.CreateAsync(request, ct), nameof(GetById), room => new { id = room.Id });

    [HttpPut("{id:int}")]
    [Authorize(Roles = AppRoles.Secretary)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateRoomRequest request, CancellationToken ct) =>
        FromResult(await _roomService.UpdateAsync(id, request, ct));

    /// <summary>Only a closed room with no bookings at all can be deleted (otherwise 409).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = AppRoles.Secretary)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct) =>
        FromResult(await _roomService.DeleteAsync(id, ct));
}
