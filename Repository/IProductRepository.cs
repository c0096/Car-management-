using Orders.Entity;

namespace Orders.Repository;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync();

    Task<Product?> GetByIdAsync(int id);

    Task<int> CreateAsync(Product product);

    Task UpdateAsync(Product product);

    Task DeleteAsync(int id);
}
