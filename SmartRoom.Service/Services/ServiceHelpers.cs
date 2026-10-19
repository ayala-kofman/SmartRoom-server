using AutoMapper;
using SmartRoom.Core.DTOs.Common;
using SmartRoom.Core.Options;

namespace SmartRoom.Service.Services;

internal static class ServiceHelpers
{
    /// <summary>The requested page size, or the default; never above the configured maximum.</summary>
    public static int ResolvePageSize(this PagingOptions paging, int? requested) =>
        Math.Min(requested ?? paging.DefaultPageSize, paging.MaxPageSize);

    /// <summary>Maps the items of a page and keeps its paging numbers.</summary>
    public static PagedResult<TDto> MapPage<TEntity, TDto>(this IMapper mapper, PagedResult<TEntity> page) => new()
    {
        Items = mapper.Map<List<TDto>>(page.Items),
        Page = page.Page,
        PageSize = page.PageSize,
        TotalCount = page.TotalCount
    };

    /// <summary>Every date in the range except Shabbat — the days that get room slots (rule 11).</summary>
    public static IEnumerable<DateOnly> SchoolDays(DateOnly from, DateOnly to)
    {
        for (var date = from; date <= to; date = date.AddDays(1))
        {
            if (date.DayOfWeek != DayOfWeek.Saturday)
                yield return date;
        }
    }
}
