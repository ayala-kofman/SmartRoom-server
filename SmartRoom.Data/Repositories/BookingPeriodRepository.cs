using Microsoft.EntityFrameworkCore;
using SmartRoom.Core.Entities;
using SmartRoom.Core.Interfaces.Repositories;

namespace SmartRoom.Data.Repositories;

public class BookingPeriodRepository : RepositoryBase, IBookingPeriodRepository
{
    public BookingPeriodRepository(AppDbContext db)
        : base(db)
    {
    }

    public async Task<IReadOnlyList<BookingPeriod>> GetAllAsync(CancellationToken ct) =>
        await Db.BookingPeriods
            .AsNoTracking()
            .Include(p => p.CreatedByUser)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<BookingPeriod>> GetOpenFromAsync(DateOnly fromDate, CancellationToken ct) =>
        await Db.BookingPeriods
            .AsNoTracking()
            .Where(p => p.EndDate >= fromDate)
            .OrderBy(p => p.StartDate)
            .ToListAsync(ct);

    public async Task AddAsync(BookingPeriod period, CancellationToken ct) => await Db.BookingPeriods.AddAsync(period, ct);
}
