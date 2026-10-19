using SmartRoom.Core.DTOs.Slots;
using SmartRoom.Core.Models;
using SmartRoom.Core.Results;

namespace SmartRoom.Core.Interfaces.Services;

public interface IBookingPeriodService
{
    Task<Result<IReadOnlyList<BookingPeriodDto>>> GetAllAsync(CancellationToken ct);

    /// <summary>
    /// Opens a period and creates its slots: school days only (no Shabbat), active rooms only, no duplicates.
    /// ValidationError if the range is invalid or starts today or in the past (rule 11).
    /// </summary>
    Task<Result<BookingPeriodDto>> CreateAsync(CreateBookingPeriodRequest request, CurrentUser currentUser, CancellationToken ct);
}
