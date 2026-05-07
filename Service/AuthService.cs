using Microsoft.Extensions.Options;
using VehicleDeclarations.Entity;
using VehicleDeclarations.Repository;

namespace VehicleDeclarations.Service;

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

        await userRepository.CreateAsync(new AppUser
        {
            Email = email,
            PasswordHash = passwordHasher.Hash(password)
        });
    }

    public async Task<AppUser?> ValidateCredentialsAsync(string email, string password)
    {
        var user = await userRepository.GetByEmailAsync(email);

        if (user is null)
        {
            return null;
        }

        return passwordHasher.Verify(password, user.PasswordHash) ? user : null;
    }
}
