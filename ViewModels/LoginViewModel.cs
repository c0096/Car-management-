using System.ComponentModel.DataAnnotations;

namespace VehicleDeclarations.ViewModels;

public sealed class LoginViewModel
{
    [Required(ErrorMessage = "L'email est obligatoire.")]
    [EmailAddress(ErrorMessage = "Adresse email invalide.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mot de passe")]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
