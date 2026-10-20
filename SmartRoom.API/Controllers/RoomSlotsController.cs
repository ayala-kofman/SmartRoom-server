using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartRoom.API.Models;
using SmartRoom.API.Security;
using SmartRoom.Core.DTOs.Common;
using SmartRoom.Core.DTOs.Slots;
using SmartRoom.Core.Interfaces.Services;

namespace SmartRoom.API.Controllers;

/// <summary>Room slots (a room, on a date, in a lesson). A teacher sees available slots only.</summary>
[Route("api/room-slots")]
[Authorize(Roles = AppRoles.SecretaryOrTeacher)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class RoomSlotsController : ApiControllerBase
{
    private readonly IRoomSlotService _slotService;

    public RoomSlotsController(IRoomSlotService slotService) => _slotService = slotService;

    [HttpGet]
    [ProducesResponseType<PagedResult<RoomSlotDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] RoomSlotQuery query, CancellationToken ct) =>
        FromResult(await _slotService.GetPagedAsync(query, CurrentUser, ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType<RoomSlotDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct) =>
        FromResult(await _slotService.GetByIdAsync(id, ct));
}
