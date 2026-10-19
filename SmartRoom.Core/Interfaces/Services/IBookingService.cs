using SmartRoom.Core.DTOs.Bookings;
using SmartRoom.Core.DTOs.Common;
using SmartRoom.Core.Models;
using SmartRoom.Core.Results;

namespace SmartRoom.Core.Interfaces.Services;

public interface IBookingService
{
    /// <summary>
    /// Books a slot (the flow in chapter 7). Created, ValidationError, Forbidden (teacher does not teach the class),
    /// NotFound, or Conflict (the slot was taken a moment ago).
    /// </summary>
    Task<Result<BookingDto>> CreateAsync(CreateBookingRequest request, CurrentUser currentUser, CancellationToken ct);

    /// <summary>Content depends on the role: a teacher gets her bookings, a student her class's, the secretary all.</summary>
    Task<Result<PagedResult<BookingDto>>> GetPagedAsync(BookingQuery query, CurrentUser currentUser, CancellationToken ct);

    /// <summary>Only the booking's teacher or the secretary; otherwise Forbidden.</summary>
    Task<Result<BookingDto>> GetByIdAsync(int id, CurrentUser currentUser, CancellationToken ct);

    /// <summary>
    /// Cancels and frees the slot. ValidationError on or after the lesson day, Forbidden for another teacher's booking,
    /// Conflict on a concurrency race.
    /// </summary>
    Task<Result> CancelAsync(int id, CurrentUser currentUser, CancellationToken ct);
}
