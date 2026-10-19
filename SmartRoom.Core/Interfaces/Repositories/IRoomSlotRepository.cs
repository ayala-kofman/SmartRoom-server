using SmartRoom.Core.DTOs.Common;
using SmartRoom.Core.DTOs.Slots;
using SmartRoom.Core.Entities;

namespace SmartRoom.Core.Interfaces.Repositories;

public interface IRoomSlotRepository
{
    /// <summary>Filtered page of slots with room and lesson period (no tracking).
    /// availableOnly = true for a teacher, regardless of query.Status.</summary>
    Task<PagedResult<RoomSlot>> GetPagedAsync(RoomSlotQuery query, int pageSize, bool availableOnly, CancellationToken ct);

    /// <summary>One slot with room and lesson period, read only. Null if not found.</summary>
    Task<RoomSlot?> GetByIdAsync(int id, CancellationToken ct);

    /// <summary>One slot with its room, TRACKED: used for booking, so EF checks Version on save. Null if not found.</summary>
    Task<RoomSlot?> GetForUpdateAsync(int id, CancellationToken ct);

    /// <summary>Existing (RoomId, Date, LessonPeriodId) keys in a date range, so new slots are not duplicated (rule 11).</summary>
    Task<HashSet<(int RoomId, DateOnly Date, int LessonPeriodId)>> GetExistingKeysAsync(
        DateOnly fromDate, DateOnly toDate, CancellationToken ct);

    Task AddRangeAsync(IEnumerable<RoomSlot> slots, CancellationToken ct);

    /// <summary>Saves all pending changes. Throws ConcurrencyConflictException on a concurrency race.</summary>
    Task SaveChangesAsync(CancellationToken ct);
}
