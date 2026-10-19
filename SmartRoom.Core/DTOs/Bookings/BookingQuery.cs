using SmartRoom.Core.DTOs.Common;

namespace SmartRoom.Core.DTOs.Bookings;

/// <summary>
/// Filters for GET api/bookings. Every filter is optional.
/// Which bookings the caller sees (own / class / all) is decided by the service from the token claims.
/// </summary>
public class BookingQuery : PagingQuery
{
    /// <summary>A single day (e.g. a student's class bookings for today).</summary>
    public DateOnly? Date { get; set; }

    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
}
