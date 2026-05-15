using Orders.Entity;

namespace Orders.Service;

public interface ICategoryService
{
    Task<IReadOnlyList<Category>> GetAllAsync();

    Task<Category?> GetByIdAsync(int id);

    Task<int> CreateAsync(Category category);

    Task UpdateAsync(Category category);

    Task DeleteAsync(int id);
}
