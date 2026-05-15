using System.ComponentModel.DataAnnotations;

namespace Orders.Entity;

public sealed class Category
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Le nom de la catégorie est obligatoire.")]
    [StringLength(120)]
    [Display(Name = "Nom")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
