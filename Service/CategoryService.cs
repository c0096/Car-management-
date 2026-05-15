using Orders.Entity;
using Orders.Repository;

namespace Orders.Service;

public sealed class CategoryService(ICategoryRepository repository) : ICategoryService
{
    public Task<IReadOnlyList<Category>> GetAllAsync()
    {
        return repository.GetAllAsync();
    }

    public Task<Category?> GetByIdAsync(int id)
    {
        return repository.GetByIdAsync(id);
    }

    public Task<int> CreateAsync(Category category)
    {
        return repository.CreateAsync(category);
    }

    public Task UpdateAsync(Category category)
    {
        return repository.UpdateAsync(category);
    }

    public async Task DeleteAsync(int id)
    {
        if (await repository.HasProductsAsync(id))
        {
            throw new InvalidOperationException("Impossible de supprimer une catégorie utilisée par des produits.");
        }

        await repository.DeleteAsync(id);
    }
}
