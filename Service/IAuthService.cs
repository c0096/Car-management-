using Orders.Entity;

namespace Orders.Service;

public interface IAuthService
{
    Task EnsureDefaultUserAsync();

    Task<User?> ValidateCredentialsAsync(string email, string password);
}
