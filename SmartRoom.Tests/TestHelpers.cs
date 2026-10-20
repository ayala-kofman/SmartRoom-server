using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using SmartRoom.Core.Options;
using SmartRoom.Service.Services;

namespace SmartRoom.Tests;

/// <summary>Shared pieces for the unit tests: a fixed clock, the real mapper and the real time rules.</summary>
public static class TestHelpers
{
    /// <summary>Wednesday 2026-10-21, 10:00 in Israel (UTC+3) — a regular school day.</summary>
    public static readonly DateTimeOffset WednesdayMorningUtc = new(2026, 10, 21, 7, 0, 0, TimeSpan.Zero);

    public static readonly DateOnly Today = new(2026, 10, 21);
    public static readonly DateOnly Tomorrow = Today.AddDays(1);

    public static FakeTimeProvider Clock(DateTimeOffset? utcNow = null) => new(utcNow ?? WednesdayMorningUtc);

    public static IOptions<BookingRulesOptions> Rules() => Options.Create(new BookingRulesOptions());

    public static IOptions<PagingOptions> Paging() => Options.Create(new PagingOptions());

    /// <summary>The real time rules on a fake clock, so "today" and "tomorrow" are fixed.</summary>
    public static BookingWindowService Window(TimeProvider clock) => new(clock, Rules());

    /// <summary>The real AutoMapper profiles of the Service project.</summary>
    public static IMapper Mapper() =>
        new MapperConfiguration(cfg => cfg.AddMaps(typeof(Service.DependencyInjection).Assembly), NullLoggerFactory.Instance)
            .CreateMapper();
}
