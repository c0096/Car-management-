using Orders.Entity;

namespace Orders.ViewModels;

public sealed class ProductIndexViewModel
{
    public IReadOnlyList<Product> Products { get; init; } = [];

    public IReadOnlyList<Category> Categories { get; init; } = [];

    public Product ProductForm { get; init; } = new();

    public Category CategoryForm { get; init; } = new();

    public string? CategoryError { get; init; }
}
