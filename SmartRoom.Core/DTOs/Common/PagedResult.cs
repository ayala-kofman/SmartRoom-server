namespace SmartRoom.Core.DTOs.Common;

/// <summary>Uniform envelope for every list response.</summary>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();

    public int Page { get; set; }
    public int PageSize { get; set; }

    /// <summary>Total matching rows in the database (separate COUNT query), not just this page.</summary>
    public int TotalCount { get; set; }

    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
