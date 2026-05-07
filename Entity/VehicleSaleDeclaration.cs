using System.ComponentModel.DataAnnotations;

namespace VehicleDeclarations.Entity;

public sealed class VehicleSaleDeclaration
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Le nom du rédacteur est obligatoire.")]
    [StringLength(150)]
    [Display(Name = "Nom du rédacteur")]
    public string WriterName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le numéro d'autorisation est obligatoire.")]
    [StringLength(80)]
    [Display(Name = "Numéro autorisation")]
    public string AuthorizationNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le téléphone du rédacteur est obligatoire.")]
    [Phone]
    [StringLength(40)]
    [Display(Name = "Téléphone du rédacteur")]
    public string WriterPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "La ville est obligatoire.")]
    [StringLength(120)]
    [Display(Name = "Ville")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "La date et l'heure sont obligatoires.")]
    [Display(Name = "Date / Heure")]
    public DateTime DeclarationDateTime { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "Le nom du vendeur est obligatoire.")]
    [StringLength(150)]
    [Display(Name = "Nom du vendeur")]
    public string SellerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'adresse du vendeur est obligatoire.")]
    [StringLength(250)]
    [Display(Name = "Adresse")]
    public string SellerAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le CIN du vendeur est obligatoire.")]
    [StringLength(80)]
    [Display(Name = "N° CIN du vendeur")]
    public string SellerCin { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le téléphone du vendeur est obligatoire.")]
    [Phone]
    [StringLength(40)]
    [Display(Name = "Téléphone du vendeur")]
    public string SellerPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "La déclaration de vente est obligatoire.")]
    [StringLength(250)]
    [Display(Name = "Déclare avoir vendu")]
    public string SoldItemDescription { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le numéro d'ordre est obligatoire.")]
    [StringLength(80)]
    [Display(Name = "Numéro d'ordre")]
    public string OrderNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le type du véhicule est obligatoire.")]
    [StringLength(120)]
    [Display(Name = "Type")]
    public string VehicleType { get; set; } = string.Empty;

    [Required(ErrorMessage = "La marque du véhicule est obligatoire.")]
    [StringLength(120)]
    [Display(Name = "Marque")]
    public string VehicleBrand { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le numéro de châssis est obligatoire.")]
    [StringLength(120)]
    [Display(Name = "Numéro châssis")]
    public string ChassisNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nom de l'acheteur est obligatoire.")]
    [StringLength(150)]
    [Display(Name = "Nom de l'acheteur")]
    public string BuyerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'adresse de l'acheteur est obligatoire.")]
    [StringLength(250)]
    [Display(Name = "Adresse de l'acheteur")]
    public string BuyerAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le CIN de l'acheteur est obligatoire.")]
    [StringLength(80)]
    [Display(Name = "N° CIN de l'acheteur")]
    public string BuyerCin { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le téléphone de l'acheteur est obligatoire.")]
    [Phone]
    [StringLength(40)]
    [Display(Name = "Téléphone de l'acheteur")]
    public string BuyerPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le titre de propriété est obligatoire.")]
    [StringLength(180)]
    [Display(Name = "Titre de propriété")]
    public string PropertyTitle { get; set; } = string.Empty;

    [StringLength(1000)]
    [Display(Name = "Observation")]
    public string? Observation { get; set; }

    [Required(ErrorMessage = "La signature du vendeur est obligatoire.")]
    [StringLength(150)]
    [Display(Name = "Signature du vendeur")]
    public string SellerSignature { get; set; } = string.Empty;

    [Required(ErrorMessage = "La signature du gérant est obligatoire.")]
    [StringLength(150)]
    [Display(Name = "Signature du gérant")]
    public string ManagerSignature { get; set; } = string.Empty;

    [Required(ErrorMessage = "La signature de l'acheteur est obligatoire.")]
    [StringLength(150)]
    [Display(Name = "Signature de l'acheteur")]
    public string BuyerSignature { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public List<DeclarationAttachment> Attachments { get; set; } = [];
}
