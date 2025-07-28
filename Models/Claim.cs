using System.ComponentModel.DataAnnotations;

namespace CoffeeShopApp.Models;

public class Claim
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string ClaimType { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string ClaimValue { get; set; } = string.Empty;

    public int UserId { get; set; }

    // Navigation property
    public virtual User User { get; set; } = null!;
}

// Static class for common claims
public static class CoffeeClaims
{
    public const string CanManageCoffee = "CanManageCoffee";
    public const string CanViewCoffee = "CanViewCoffee";
    public const string IsBarista = "IsBarista";
    public const string IsManager = "IsManager";
}