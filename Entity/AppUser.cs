using System.ComponentModel.DataAnnotations;

namespace VehicleDeclarations.Entity;

public sealed class AppUser
{
    public int Id { get; set; }

    [Required(ErrorMessage = "L'email est obligatoire.")]
    [EmailAddress(ErrorMessage = "Adresse email invalide.")]
    [StringLength(256)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
