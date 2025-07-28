using System;
using System.ComponentModel.DataAnnotations;

namespace CoffeeShopApp.Models;

public class Coffee
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, 999.99)]
    public decimal Price { get; set; }

    [StringLength(50)]
    public string Origin { get; set; } = string.Empty;

    [StringLength(20)]
    public string RoastLevel { get; set; } = string.Empty;

    public bool IsAvailable { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}