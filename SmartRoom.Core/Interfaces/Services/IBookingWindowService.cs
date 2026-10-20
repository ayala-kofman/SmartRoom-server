namespace SmartRoom.Core.Interfaces.Services;

/// <summary>
/// Time rules in school (Israel) time, based on an injected clock (TimeProvider) and BookingRulesOptions,
/// so tests can fake "now".
/// </summary>
public interface IBookingWindowService
{
    /// <summary>True between ShabbatCloseTime on Friday and ShabbatOpenTime on Saturday night.</summary>
    bool IsSiteClosedNow();

    /// <summary>Today's date in school time.</summary>
    DateOnly GetToday();

    /// <summary>The only date that may be booked now (today + DaysAhead).</summary>
    DateOnly GetBookableDate();

    /// <summary>True once the lesson that starts at this date and time (school time) has begun, or is in the past.</summary>
    bool HasStarted(DateOnly date, TimeOnly startTime);
}
