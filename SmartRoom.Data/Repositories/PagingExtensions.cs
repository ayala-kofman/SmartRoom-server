using Microsoft.EntityFrameworkCore;
using SmartRoom.Core.DTOs.Common;

namespace SmartRoom.Data.Repositories;

public static class PagingExtensions
{
    /// <summary>Runs a COUNT query and a page query (Skip/Take) on an already filtered and sorted query.</summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> source, int page, int pageSize, CancellationToken ct)
    {
        var totalCount = await source.CountAsync(ct);
        var items = await source
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
