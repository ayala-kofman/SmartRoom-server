using AutoMapper;
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

    public RoomSlotService(IRoomSlotRepository slots, IMapper mapper, IOptions<PagingOptions> paging)
    {
        _slots = slots;
        _mapper = mapper;
        _paging = paging.Value;
    }

    public async Task<Result<PagedResult<RoomSlotDto>>> GetPagedAsync(RoomSlotQuery query, CurrentUser currentUser, CancellationToken ct)
    {
        // Decided by the role in the token, not by what the client sends.
        var availableOnly = currentUser.Role != UserRole.Secretary;

        var page = await _slots.GetPagedAsync(query, _paging.ResolvePageSize(query.PageSize), availableOnly, ct);
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
