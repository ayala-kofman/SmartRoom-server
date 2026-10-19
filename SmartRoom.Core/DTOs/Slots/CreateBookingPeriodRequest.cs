using System.ComponentModel.DataAnnotations;

namespace SmartRoom.Core.DTOs.Slots;

/// <summary>
/// Body of POST api/booking-periods. Opening a period creates its room slots.
/// The service checks that EndDate is not before StartDate.
/// </summary>
public class CreateBookingPeriodRequest
{
    [Required]
    public DateOnly? StartDate { get; set; }

    [Required]
    public DateOnly? EndDate { get; set; }
}
