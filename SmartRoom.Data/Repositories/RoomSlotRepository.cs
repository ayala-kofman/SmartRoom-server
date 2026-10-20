using Microsoft.EntityFrameworkCore;
using SmartRoom.Core.DTOs.Common;
using SmartRoom.Core.DTOs.Slots;
using SmartRoom.Core.Entities;
using SmartRoom.Core.Enums;
using SmartRoom.Core.Interfaces.Repositories;

namespace SmartRoom.Data.Repositories;

public class RoomSlotRepository : RepositoryBase, IRoomSlotRepository
{
    public RoomSlotRepository(AppDbContext db)
        : base(db)
    {
    }

    public async Task<PagedResult<RoomSlot>> GetPagedAsync(
        RoomSlotQuery query, int pageSize, bool availableOnly, DateTime? startsAfter, CancellationToken ct)
    {
        IQueryable<RoomSlot> slots = Db.RoomSlots
            .AsNoTracking()
            .Include(s => s.Room)
            .Include(s => s.LessonPeriod);

        if (query.Date is DateOnly date)
            slots = slots.Where(s => s.Date == date);

        if (query.LessonPeriodId is int lessonPeriodId)
            slots = slots.Where(s => s.LessonPeriodId == lessonPeriodId);

        if (query.RoomType is RoomType roomType)
            slots = slots.Where(s => EF.Property<RoomType>(s.Room, "RoomType") == roomType);

        if (query.MinQuality is QualityLevel minQuality)
            slots = slots.Where(s => s.Room.Quality >= minQuality);

        if (query.MinCapacity is int minCapacity)
            slots = slots.Where(s =>
                (s.Room is RegularRoom ? ((RegularRoom)s.Room).SeatCount : ((ComputerRoom)s.Room).ComputerCount) >= minCapacity);

        // A teacher sees available slots only, whatever status she asked for.
        var status = availableOnly ? SlotStatus.Available : query.Status;
        if (status is SlotStatus slotStatus)
            slots = slots.Where(s => s.Status == slotStatus);

        // Today's lessons that already started cannot be booked, so they are not shown.
        if (startsAfter is DateTime now)
        {
            var today = DateOnly.FromDateTime(now);
            var time = TimeOnly.FromDateTime(now);
            slots = slots.Where(s => s.Date > today || (s.Date == today && s.LessonPeriod.StartTime > time));
        }

        // Earliest first; within the same lesson, best room first.
        slots = slots
            .OrderBy(s => s.Date)
            .ThenBy(s => s.LessonPeriod.LessonNumber)
            .ThenByDescending(s => s.Room.Quality)
            .ThenBy(s => s.Id);

        return await slots.ToPagedResultAsync(query.Page, pageSize, ct);
    }

    public Task<RoomSlot?> GetByIdAsync(int id, CancellationToken ct) =>
        Db.RoomSlots
            .AsNoTracking()
            .Include(s => s.Room)
            .Include(s => s.LessonPeriod)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<RoomSlot?> GetForUpdateAsync(int id, CancellationToken ct) =>
        Db.RoomSlots
            .Include(s => s.Room)
            .Include(s => s.LessonPeriod)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<HashSet<(int RoomId, DateOnly Date, int LessonPeriodId)>> GetExistingKeysAsync(
        DateOnly fromDate, DateOnly toDate, CancellationToken ct)
    {
        var keys = await Db.RoomSlots
            .Where(s => s.Date >= fromDate && s.Date <= toDate)
            .Select(s => new { s.RoomId, s.Date, s.LessonPeriodId })
            .ToListAsync(ct);

        return keys.Select(k => (k.RoomId, k.Date, k.LessonPeriodId)).ToHashSet();
    }

    public Task AddRangeAsync(IEnumerable<RoomSlot> slots, CancellationToken ct) =>
        Db.RoomSlots.AddRangeAsync(slots, ct);
}
