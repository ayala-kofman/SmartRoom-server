using System.ComponentModel.DataAnnotations;

namespace SmartRoom.Core.DTOs.Common;

/// <summary>
/// Paging parameters from the query string. Other query DTOs inherit from it.
/// The upper limit on PageSize comes from PagingOptions and is enforced in the service.
/// </summary>
public class PagingQuery
{
    [Range(1, int.MaxValue, ErrorMessage = "Page must be 1 or greater.")]
    public int Page { get; set; } = 1;

    /// <summary>Null means "use the configured default page size".</summary>
    [Range(1, int.MaxValue, ErrorMessage = "PageSize must be 1 or greater.")]
    public int? PageSize { get; set; }
}
