using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartRoom.Core.DTOs.Common;
using SmartRoom.Core.DTOs.Slots;
using SmartRoom.Core.Entities;
using SmartRoom.Core.Enums;
using SmartRoom.Core.Interfaces.Repositories;
using SmartRoom.Core.Interfaces.Services;
using SmartRoom.Core.Models;
using SmartRoom.Core.Options;
using SmartRoom.Core.Results;

namespace SmartRoom.Service.Services;

public class RoomSlotService : IRoomSlotService
{
    private readonly IRoomSlotRepository _slots;
    private readonly IMapper _mapper;
    private readonly PagingOptions _paging;
    private readonly ILogger<RoomSlotService> _logger;

    public RoomSlotService(IRoomSlotRepository slots, IMapper mapper, IOptions<PagingOptions> paging, ILogger<RoomSlotService> logger)
    {
        _slots = slots;
        _mapper = mapper;
        _paging = paging.Value;
        _logger = logger;
    }

    public async Task<Result<PagedResult<RoomSlotDto>>> GetPagedAsync(RoomSlotQuery query, CurrentUser currentUser, CancellationToken ct)
    {
        // Decided by the role in the token, not by what the client sends.
        var availableOnly = currentUser.Role != UserRole.Secretary;

        var page = await _slots.GetPagedAsync(query, _paging.ResolvePageSize(query.PageSize), availableOnly, ct);
        _logger.LogDebug(
            "Slots query by {Role}: date {Date:yyyy-MM-dd}, lesson {LessonPeriodId}, type {RoomType}, min quality {MinQuality}, min capacity {MinCapacity}, status {Status}, available only {AvailableOnly}, page {Page}: {TotalCount} found.",
            currentUser.Role, query.Date, query.LessonPeriodId, query.RoomType, query.MinQuality, query.MinCapacity, query.Status,
            availableOnly, page.Page, page.TotalCount);
        return Result<PagedResult<RoomSlotDto>>.Success(_mapper.MapPage<RoomSlot, RoomSlotDto>(page));
    }

    public async Task<Result<RoomSlotDto>> GetByIdAsync(int id, CancellationToken ct)
    {
        var slot = await _slots.GetByIdAsync(id, ct);
        return slot is null
            ? Result<RoomSlotDto>.NotFound($"Slot {id} was not found.")
            : Result<RoomSlotDto>.Success(_mapper.Map<RoomSlotDto>(slot));
    }
}
