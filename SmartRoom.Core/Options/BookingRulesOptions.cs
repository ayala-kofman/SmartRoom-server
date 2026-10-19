namespace SmartRoom.Core.Options;

/// <summary>Booking rules from appsettings.json. All times are school (Israel) local time.</summary>
public class BookingRulesOptions
{
    public const string SectionName = "BookingRules";

    /// <summary>How many days ahead a slot may be booked. 1 = tomorrow only.</summary>
    public int DaysAhead { get; set; } = 1;

    /// <summary>"Israel Standard Time" on Windows, "Asia/Jerusalem" on Linux; both work on .NET 8.</summary>
    public string TimeZoneId { get; set; } = "Asia/Jerusalem";

    /// <summary>The site closes on Friday at this time (a safe margin for the whole year).</summary>
    public TimeOnly ShabbatCloseTime { get; set; } = new(12, 0);

    /// <summary>The site reopens on Saturday night at this time (late enough for the whole year).</summary>
    public TimeOnly ShabbatOpenTime { get; set; } = new(21, 0);
}
