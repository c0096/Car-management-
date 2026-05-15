using System.ComponentModel.DataAnnotations;

namespace Orders.Entity;

public sealed class Product
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Le nom du produit est obligatoire.")]
    [StringLength(160)]
    [Display(Name = "Nom")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "La référence est obligatoire.")]
    [StringLength(80)]
    [Display(Name = "Référence")]
    public string Sku { get; set; } = string.Empty;

    [StringLength(700)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "La catégorie est obligatoire.")]
    [Display(Name = "Catégorie")]
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    [Range(0, 999999999, ErrorMessage = "Le prix doit être positif.")]
    [Display(Name = "Prix unitaire")]
    public decimal UnitPrice { get; set; }

    [Range(0, 999999, ErrorMessage = "Le stock doit être positif.")]
    [Display(Name = "Stock")]
    public int StockQuantity { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
