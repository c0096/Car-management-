namespace VehicleDeclarations.Entity;

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];

    public int TotalItems { get; init; }

    public int Page { get; init; }

    public int PageSize { get; init; }

    public int TotalPages => TotalItems == 0 ? 1 : (int)Math.Ceiling(TotalItems / (double)PageSize);
}
