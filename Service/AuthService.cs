using Microsoft.Extensions.Options;
using Orders.Entity;
using Orders.Repository;

namespace Orders.Service;

public sealed class AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, IOptions<AuthOptions> options) : IAuthService
{
    public async Task EnsureDefaultUserAsync()
    {
        if (await userRepository.AnyAsync())
        {
            return;
        }

        var email = options.Value.DefaultEmail.Trim();
        var password = options.Value.DefaultPassword;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("Default authentication credentials are missing.");
        }

        await userRepository.CreateAsync(new User
        {
            Email = email,
            PasswordHash = passwordHasher.Hash(password)
        });
    }

    public async Task<User?> ValidateCredentialsAsync(string email, string password)
    {
        var user = await userRepository.GetByEmailAsync(email);

        if (user is null)
        {
            return null;
        }

        return passwordHasher.Verify(password, user.PasswordHash) ? user : null;
    }
}
