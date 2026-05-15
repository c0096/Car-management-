using Orders.Entity;

namespace Orders.Repository;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetAllAsync();

    Task<Category?> GetByIdAsync(int id);

    Task<int> CreateAsync(Category category);

    Task UpdateAsync(Category category);

    Task DeleteAsync(int id);

    Task<bool> HasProductsAsync(int id);
}
