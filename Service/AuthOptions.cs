namespace Orders.Service;

public sealed class AuthOptions
{
    public string DefaultEmail { get; set; } = "admin@example.com";

    public string DefaultPassword { get; set; } = "Admin123!";
}
