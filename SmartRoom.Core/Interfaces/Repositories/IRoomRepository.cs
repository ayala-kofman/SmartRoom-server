using SmartRoom.Core.DTOs.Common;
using SmartRoom.Core.DTOs.Rooms;
using SmartRoom.Core.Entities;

namespace SmartRoom.Core.Interfaces.Repositories;

public interface IRoomRepository
{
    /// <summary>Filtered, sorted page of rooms with their features (no tracking).
    /// pageSize is already resolved by the service from PagingOptions.</summary>
    Task<PagedResult<Room>> GetPagedAsync(RoomQuery query, int pageSize, CancellationToken ct);

    /// <summary>One room with its features, read only (no tracking). Null if not found.</summary>
    Task<Room?> GetByIdAsync(int id, CancellationToken ct);

    /// <summary>One room with its features, tracked, for update or delete. Null if not found.</summary>
    Task<Room?> GetForUpdateAsync(int id, CancellationToken ct);

    /// <summary>True if another room already uses this number (excludeRoomId skips the room being updated).</summary>
    Task<bool> RoomNumberExistsAsync(string roomNumber, int? excludeRoomId, CancellationToken ct);

    /// <summary>True if the room has any booking at all, past or cancelled included (blocks delete, rule 9).</summary>
    Task<bool> HasAnyBookingsAsync(int roomId, CancellationToken ct);

    Task AddAsync(Room room, CancellationToken ct);
    void Remove(Room room);

    /// <summary>Saves all pending changes. Throws ConcurrencyConflictException on a concurrency race.</summary>
    Task SaveChangesAsync(CancellationToken ct);
}
