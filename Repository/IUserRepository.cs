using Orders.Entity;

namespace Orders.Repository;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);

    Task<bool> AnyAsync();

    Task<int> CreateAsync(User user);
}
