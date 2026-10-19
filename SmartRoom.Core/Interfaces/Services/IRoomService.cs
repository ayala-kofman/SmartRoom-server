using SmartRoom.Core.DTOs.Common;
using SmartRoom.Core.DTOs.Rooms;
using SmartRoom.Core.Results;

namespace SmartRoom.Core.Interfaces.Services;

public interface IRoomService
{
    Task<Result<PagedResult<RoomDto>>> GetPagedAsync(RoomQuery query, CancellationToken ct);

    Task<Result<RoomDto>> GetByIdAsync(int id, CancellationToken ct);

    /// <summary>Created; ValidationError if the field for the room type is missing; Conflict if the room number exists.
    /// Also creates slots for all open booking periods (rule 12).</summary>
    Task<Result<RoomDto>> CreateAsync(CreateRoomRequest request, CancellationToken ct);

    Task<Result> UpdateAsync(int id, UpdateRoomRequest request, CancellationToken ct);

    /// <summary>NotFound, or Conflict if the room has future active bookings.</summary>
    Task<Result> DeleteAsync(int id, CancellationToken ct);
}
