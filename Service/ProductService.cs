using Orders.Entity;
using Orders.Repository;

namespace Orders.Service;

public sealed class ProductService(IProductRepository repository, ICategoryRepository categoryRepository) : IProductService
{
    public Task<IReadOnlyList<Product>> GetAllAsync()
    {
        return repository.GetAllAsync();
    }

    public Task<Product?> GetByIdAsync(int id)
    {
        return repository.GetByIdAsync(id);
    }

    public async Task<int> CreateAsync(Product product)
    {
        await EnsureCategoryExists(product.CategoryId);
        return await repository.CreateAsync(product);
    }

    public async Task UpdateAsync(Product product)
    {
        await EnsureCategoryExists(product.CategoryId);
        await repository.UpdateAsync(product);
    }

    public Task DeleteAsync(int id)
    {
        return repository.DeleteAsync(id);
    }

    private async Task EnsureCategoryExists(int categoryId)
    {
        if (categoryId <= 0 || await categoryRepository.GetByIdAsync(categoryId) is null)
        {
            throw new InvalidOperationException("La catégorie sélectionnée est introuvable.");
        }
    }
}
