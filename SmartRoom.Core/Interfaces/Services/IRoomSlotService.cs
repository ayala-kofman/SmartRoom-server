using SmartRoom.Core.DTOs.Common;
using SmartRoom.Core.DTOs.Slots;
using SmartRoom.Core.Models;
using SmartRoom.Core.Results;

namespace SmartRoom.Core.Interfaces.Services;

public interface IRoomSlotService
{
    /// <summary>The secretary sees every status; a teacher sees available slots only.</summary>
    Task<Result<PagedResult<RoomSlotDto>>> GetPagedAsync(RoomSlotQuery query, CurrentUser currentUser, CancellationToken ct);

    Task<Result<RoomSlotDto>> GetByIdAsync(int id, CancellationToken ct);
}
