namespace SmartRoom.Tests.Services;

/// <summary>The Shabbat window by Israel time (defaults: closes Friday 12:00, opens Saturday 21:00).</summary>
public class BookingWindowServiceTests
{
    // Week of 2026-10-23 (Friday) / 2026-10-24 (Saturday); Israel is UTC+3 then.
    private static DateTimeOffset IsraelTime(int day, int hour, int minute = 0) =>
        new DateTimeOffset(2026, 10, day, hour, minute, 0, TimeSpan.FromHours(3)).ToUniversalTime();

    private static bool IsClosedAt(DateTimeOffset utc) => TestHelpers.Window(TestHelpers.Clock(utc)).IsSiteClosedNow();

    [Fact]
    public void IsSiteClosedNow_FridayMorning_IsOpen() => Assert.False(IsClosedAt(IsraelTime(23, 9)));

    [Fact]
    public void IsSiteClosedNow_FridayAtCloseTime_IsClosed() => Assert.True(IsClosedAt(IsraelTime(23, 12)));

    [Fact]
    public void IsSiteClosedNow_SaturdayNoon_IsClosed() => Assert.True(IsClosedAt(IsraelTime(24, 12)));

    [Fact]
    public void IsSiteClosedNow_SaturdayJustBeforeOpenTime_IsClosed() => Assert.True(IsClosedAt(IsraelTime(24, 20, 59)));

    [Fact]
    public void IsSiteClosedNow_SaturdayNightAfterOpenTime_IsOpen() => Assert.False(IsClosedAt(IsraelTime(24, 21)));

    [Fact]
    public void IsSiteClosedNow_SundayMorning_IsOpen() => Assert.False(IsClosedAt(IsraelTime(25, 8)));

    [Fact]
    public void IsSiteClosedNow_FridayMorningUtcButAfternoonInIsrael_UsesIsraelTime()
    {
        // 10:00 UTC is 13:00 in Israel: closed, although the UTC clock says morning.
        Assert.True(IsClosedAt(new DateTimeOffset(2026, 10, 23, 10, 0, 0, TimeSpan.Zero)));
    }

    // The default clock is Wednesday 2026-10-21, 10:00 in Israel.
    [Theory]
    [InlineData(0, 9, 59, true)]   // today, a minute ago
    [InlineData(0, 10, 0, true)]   // today, right now
    [InlineData(0, 10, 1, false)]  // today, in a minute
    [InlineData(-1, 17, 30, true)] // yesterday
    [InlineData(1, 8, 0, false)]   // tomorrow
    public void HasStarted_ComparesWithIsraelNow(int daysFromToday, int hour, int minute, bool expected)
    {
        var window = TestHelpers.Window(TestHelpers.Clock());

        Assert.Equal(expected, window.HasStarted(TestHelpers.Today.AddDays(daysFromToday), new TimeOnly(hour, minute)));
    }

    [Fact]
    public void GetBookableDate_OnWednesday_IsTomorrow()
    {
        var window = TestHelpers.Window(TestHelpers.Clock());

        Assert.Equal(TestHelpers.Today, window.GetToday());
        Assert.Equal(TestHelpers.Tomorrow, window.GetBookableDate());
    }

    // The default clock is Wednesday 2026-10-21, 10:00 in Israel.
    [Theory]
    [InlineData(0, 10, 1, true)]   // today, starts in a minute
    [InlineData(0, 10, 0, false)]  // today, starts right now
    [InlineData(0, 8, 0, false)]   // today, already started
    [InlineData(1, 8, 0, true)]    // tomorrow
    [InlineData(2, 8, 0, false)]   // the day after tomorrow
    [InlineData(-1, 17, 30, false)] // yesterday
    public void CanBook_TodayBeforeTheLessonOrTomorrow(int daysFromToday, int hour, int minute, bool expected)
    {
        var window = TestHelpers.Window(TestHelpers.Clock());

        Assert.Equal(expected, window.CanBook(TestHelpers.Today.AddDays(daysFromToday), new TimeOnly(hour, minute)));
    }
}
