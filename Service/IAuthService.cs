using VehicleDeclarations.Entity;

namespace VehicleDeclarations.Service;

public interface IAuthService
{
    Task EnsureDefaultUserAsync();

    Task<AppUser?> ValidateCredentialsAsync(string email, string password);
}
