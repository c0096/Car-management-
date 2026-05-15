namespace Orders.Entity;

public sealed class SearchOptions
{
    public string? Search { get; set; }

    public string Sort { get; set; } = "date";

    public string Direction { get; set; } = "desc";

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public int Offset => (Page - 1) * PageSize;
}
