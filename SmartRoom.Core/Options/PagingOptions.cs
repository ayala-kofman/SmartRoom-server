namespace SmartRoom.Core.Options;

/// <summary>Paging limits from appsettings.json, used when a query does not send a page size or sends one that is too large.</summary>
public class PagingOptions
{
    public const string SectionName = "Paging";

    public int DefaultPageSize { get; set; } = 10;
    public int MaxPageSize { get; set; } = 50;
}
