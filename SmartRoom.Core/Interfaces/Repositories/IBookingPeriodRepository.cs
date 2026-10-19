using SmartRoom.Core.Entities;

namespace SmartRoom.Core.Interfaces.Repositories;

public interface IBookingPeriodRepository
{
    /// <summary>All periods with the user who opened them, newest first (no tracking).</summary>
    Task<IReadOnlyList<BookingPeriod>> GetAllAsync(CancellationToken ct);

    /// <summary>Periods that end on or after the given date; a new room gets slots for these (rule 12).</summary>
    Task<IReadOnlyList<BookingPeriod>> GetOpenFromAsync(DateOnly fromDate, CancellationToken ct);

    Task AddAsync(BookingPeriod period, CancellationToken ct);

    /// <summary>Saves all pending changes. Throws ConcurrencyConflictException on a concurrency race.</summary>
    Task SaveChangesAsync(CancellationToken ct);
}
