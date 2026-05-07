using VehicleDeclarations.Entity;

namespace VehicleDeclarations.Repository;

public interface IUserRepository
{
    Task<AppUser?> GetByEmailAsync(string email);

    Task<bool> AnyAsync();

    Task<int> CreateAsync(AppUser user);
}
