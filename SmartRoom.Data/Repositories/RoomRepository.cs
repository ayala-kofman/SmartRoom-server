using Microsoft.EntityFrameworkCore;
using SmartRoom.Core.DTOs.Common;
using SmartRoom.Core.DTOs.Rooms;
using SmartRoom.Core.Entities;
using SmartRoom.Core.Enums;
using SmartRoom.Core.Interfaces.Repositories;

namespace SmartRoom.Data.Repositories;

public class RoomRepository : RepositoryBase, IRoomRepository
{
    public RoomRepository(AppDbContext db)
        : base(db)
    {
    }

    public async Task<PagedResult<Room>> GetPagedAsync(RoomQuery query, int pageSize, CancellationToken ct)
    {
        IQueryable<Room> rooms = Db.Rooms.AsNoTracking().Include(r => r.Features);

        if (query.RoomType is RoomType roomType)
            rooms = rooms.Where(r => EF.Property<RoomType>(r, "RoomType") == roomType);

        if (query.Floor is int floor)
            rooms = rooms.Where(r => r.Floor == floor);

        if (query.MinQuality is QualityLevel minQuality)
            rooms = rooms.Where(r => r.Quality >= minQuality);

        if (query.MinCapacity is int minCapacity)
            rooms = rooms.Where(r => (r is RegularRoom ? ((RegularRoom)r).SeatCount : ((ComputerRoom)r).ComputerCount) >= minCapacity);

        if (query.Status is RoomStatus status)
            rooms = rooms.Where(r => r.Status == status);

        // Room must have ALL requested features: one condition per feature.
        foreach (var featureId in query.FeatureIds.Distinct())
            rooms = rooms.Where(r => r.Features.Any(f => f.Id == featureId));

        rooms = Sort(rooms, query.SortBy, query.Descending);

        return await rooms.ToPagedResultAsync(query.Page, pageSize, ct);
    }

    public Task<Room?> GetByIdAsync(int id, CancellationToken ct) =>
        Db.Rooms.AsNoTracking().Include(r => r.Features).FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<Room?> GetForUpdateAsync(int id, CancellationToken ct) =>
        Db.Rooms.Include(r => r.Features).FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<bool> RoomNumberExistsAsync(string roomNumber, int? excludeRoomId, CancellationToken ct) =>
        Db.Rooms.AnyAsync(r => r.RoomNumber == roomNumber && r.Id != excludeRoomId, ct);

    public Task<bool> HasAnyBookingsAsync(int roomId, CancellationToken ct) =>
        Db.Bookings.AnyAsync(b => b.RoomSlot.RoomId == roomId, ct);

    public async Task<IReadOnlyList<int>> GetActiveRoomIdsAsync(CancellationToken ct) =>
        await Db.Rooms.Where(r => r.Status == RoomStatus.Active).Select(r => r.Id).ToListAsync(ct);

    public async Task AddAsync(Room room, CancellationToken ct) => await Db.Rooms.AddAsync(room, ct);

    public void Remove(Room room) => Db.Rooms.Remove(room);

    /// <summary>Id is the last tie-breaker, so paging is stable when sort values are equal.</summary>
    private static IQueryable<Room> Sort(IQueryable<Room> rooms, RoomSortBy sortBy, bool descending) =>
        (sortBy, descending) switch
        {
            (RoomSortBy.RoomNumber, false) => rooms.OrderBy(r => r.RoomNumber),
            (RoomSortBy.RoomNumber, true) => rooms.OrderByDescending(r => r.RoomNumber),
            (RoomSortBy.Floor, false) => rooms.OrderBy(r => r.Floor).ThenBy(r => r.Id),
            (RoomSortBy.Floor, true) => rooms.OrderByDescending(r => r.Floor).ThenBy(r => r.Id),
            (RoomSortBy.Capacity, false) => rooms
                .OrderBy(r => r is RegularRoom ? ((RegularRoom)r).SeatCount : ((ComputerRoom)r).ComputerCount)
                .ThenBy(r => r.Id),
            (RoomSortBy.Capacity, true) => rooms
                .OrderByDescending(r => r is RegularRoom ? ((RegularRoom)r).SeatCount : ((ComputerRoom)r).ComputerCount)
                .ThenBy(r => r.Id),
            (_, false) => rooms.OrderBy(r => r.Quality).ThenBy(r => r.Id),
            _ => rooms.OrderByDescending(r => r.Quality).ThenBy(r => r.Id)
        };
}
