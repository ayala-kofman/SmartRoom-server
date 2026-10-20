using Microsoft.Extensions.Options;
using SmartRoom.Core.Interfaces.Services;
using SmartRoom.Core.Options;

namespace SmartRoom.Service.Services;

/// <summary>
/// Time rules in school (Israel) time. The check is always by Israel time, not by the user's clock:
/// the site follows the school's hours, wherever the user is.
/// </summary>
public class BookingWindowService : IBookingWindowService
{
    private readonly TimeProvider _time;
    private readonly BookingRulesOptions _rules;
    private readonly TimeZoneInfo _schoolZone;

    public BookingWindowService(TimeProvider time, IOptions<BookingRulesOptions> rules)
    {
        _time = time;
        _rules = rules.Value;
        _schoolZone = TimeZoneInfo.FindSystemTimeZoneById(_rules.TimeZoneId);
    }

    public bool IsSiteClosedNow()
    {
        var now = GetSchoolNow();
        var timeOfDay = TimeOnly.FromDateTime(now);

        return now.DayOfWeek switch
        {
            DayOfWeek.Friday => timeOfDay >= _rules.ShabbatCloseTime,
            DayOfWeek.Saturday => timeOfDay < _rules.ShabbatOpenTime,
            _ => false
        };
    }

    public DateOnly GetToday() => DateOnly.FromDateTime(GetSchoolNow());

    public DateOnly GetBookableDate() => GetToday().AddDays(_rules.DaysAhead);

    public bool HasStarted(DateOnly date, TimeOnly startTime) => date.ToDateTime(startTime) <= GetSchoolNow();

    private DateTime GetSchoolNow() => TimeZoneInfo.ConvertTime(_time.GetUtcNow(), _schoolZone).DateTime;
}
