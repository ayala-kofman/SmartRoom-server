using SmartRoom.Core.DTOs.Bookings;
using SmartRoom.Core.DTOs.Common;
using SmartRoom.Core.Entities;

namespace SmartRoom.Core.Interfaces.Repositories;

public interface IBookingRepository
{
    /// <summary>
    /// Filtered page of bookings with slot → room, slot → lesson period, teacher and class (no tracking).
    /// teacherId / classGroupId come from the token claims (set by the service); null = no restriction (secretary).
    /// </summary>
    Task<PagedResult<Booking>> GetPagedAsync(
        BookingQuery query, int pageSize, int? teacherId, int? classGroupId, CancellationToken ct);

    /// <summary>One booking with all related data, read only. Null if not found.</summary>
    Task<Booking?> GetByIdAsync(int id, CancellationToken ct);

    /// <summary>One booking with its slot, TRACKED: used for cancel (the slot goes back to available). Null if not found.</summary>
    Task<Booking?> GetForUpdateAsync(int id, CancellationToken ct);

    /// <summary>True if the teacher already has an active booking at this date and lesson period.</summary>
    Task<bool> TeacherHasBookingAtAsync(int teacherId, DateOnly date, int lessonPeriodId, CancellationToken ct);

    /// <summary>True if the class already has an active booking at this date and lesson period.</summary>
    Task<bool> ClassHasBookingAtAsync(int classGroupId, DateOnly date, int lessonPeriodId, CancellationToken ct);

    Task AddAsync(Booking booking, CancellationToken ct);

    /// <summary>
    /// Saves all pending changes (slot + booking in one transaction).
    /// Throws ConcurrencyConflictException on a Version mismatch or an active-booking unique index violation.
    /// </summary>
    Task SaveChangesAsync(CancellationToken ct);
}
