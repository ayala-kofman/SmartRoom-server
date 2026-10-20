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

    [Fact]
    public void GetBookableDate_OnWednesday_IsTomorrow()
    {
        var window = TestHelpers.Window(TestHelpers.Clock());

        Assert.Equal(TestHelpers.Today, window.GetToday());
        Assert.Equal(TestHelpers.Tomorrow, window.GetBookableDate());
    }
}
