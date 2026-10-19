using Microsoft.EntityFrameworkCore;
using SmartRoom.Core.DTOs.Bookings;
using SmartRoom.Core.DTOs.Common;
using SmartRoom.Core.Entities;
using SmartRoom.Core.Interfaces.Repositories;

namespace SmartRoom.Data.Repositories;

public class BookingRepository : RepositoryBase, IBookingRepository
{
    public BookingRepository(AppDbContext db)
        : base(db)
    {
    }

    public async Task<PagedResult<Booking>> GetPagedAsync(
        BookingQuery query, int pageSize, int? teacherId, int? classGroupId, CancellationToken ct)
    {
        var bookings = WithDetails(Db.Bookings.AsNoTracking());

        if (teacherId is int tId)
            bookings = bookings.Where(b => b.TeacherId == tId);

        if (classGroupId is int cId)
            bookings = bookings.Where(b => b.ClassGroupId == cId);

        if (query.Date is DateOnly date)
            bookings = bookings.Where(b => b.RoomSlot.Date == date);

        if (query.FromDate is DateOnly fromDate)
            bookings = bookings.Where(b => b.RoomSlot.Date >= fromDate);

        if (query.ToDate is DateOnly toDate)
            bookings = bookings.Where(b => b.RoomSlot.Date <= toDate);

        bookings = bookings
            .OrderBy(b => b.RoomSlot.Date)
            .ThenBy(b => b.RoomSlot.LessonPeriod.LessonNumber)
            .ThenBy(b => b.Id);

        return await bookings.ToPagedResultAsync(query.Page, pageSize, ct);
    }

    public Task<Booking?> GetByIdAsync(int id, CancellationToken ct) =>
        WithDetails(Db.Bookings.AsNoTracking()).FirstOrDefaultAsync(b => b.Id == id, ct);

    public Task<Booking?> GetForUpdateAsync(int id, CancellationToken ct) =>
        WithDetails(Db.Bookings).FirstOrDefaultAsync(b => b.Id == id, ct);

    public Task<bool> TeacherHasBookingAtAsync(int teacherId, DateOnly date, int lessonPeriodId, CancellationToken ct) =>
        Db.Bookings.AnyAsync(b =>
            b.IsActive
            && b.TeacherId == teacherId
            && b.RoomSlot.Date == date
            && b.RoomSlot.LessonPeriodId == lessonPeriodId, ct);

    public Task<bool> ClassHasBookingAtAsync(int classGroupId, DateOnly date, int lessonPeriodId, CancellationToken ct) =>
        Db.Bookings.AnyAsync(b =>
            b.IsActive
            && b.ClassGroupId == classGroupId
            && b.RoomSlot.Date == date
            && b.RoomSlot.LessonPeriodId == lessonPeriodId, ct);

    public async Task AddAsync(Booking booking, CancellationToken ct) => await Db.Bookings.AddAsync(booking, ct);

    /// <summary>Everything the flat BookingDto needs, loaded in one query (no N+1).</summary>
    private static IQueryable<Booking> WithDetails(IQueryable<Booking> bookings) =>
        bookings
            .Include(b => b.RoomSlot).ThenInclude(s => s.Room)
            .Include(b => b.RoomSlot).ThenInclude(s => s.LessonPeriod)
            .Include(b => b.Teacher)
            .Include(b => b.ClassGroup);
}
