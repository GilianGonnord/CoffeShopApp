using System;
using CoffeeShopApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShopApp.Data;

public class CoffeeShopContext : DbContext
{
    public CoffeeShopContext(DbContextOptions<CoffeeShopContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Claim> Claims { get; set; }
    public DbSet<Coffee> Coffees { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).IsRequired();
        });

        // Configure Claim entity
        modelBuilder.Entity<Claim>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ClaimType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ClaimValue).IsRequired().HasMaxLength(200);

            entity.HasOne(e => e.User)
                  .WithMany(u => u.Claims)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Coffee entity
        modelBuilder.Entity<Coffee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Origin).HasMaxLength(50);
            entity.Property(e => e.RoastLevel).HasMaxLength(20);
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed a default admin user
        var adminUser = new User
        {
            Id = 1,
            Username = "admin",
            Email = "admin@coffeeshop.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            CreatedAt = DateTime.UtcNow
        };

        var baristaUser = new User
        {
            Id = 2,
            Username = "barista",
            Email = "barista@coffeeshop.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("barista123"),
            CreatedAt = DateTime.UtcNow
        };

        modelBuilder.Entity<User>().HasData(adminUser, baristaUser);

        // Seed claims
        var adminClaims = new[]
        {
                new Claim { Id = 1, UserId = 1, ClaimType = CoffeeClaims.IsManager, ClaimValue = "true" },
                new Claim { Id = 2, UserId = 1, ClaimType = CoffeeClaims.CanManageCoffee, ClaimValue = "true" },
                new Claim { Id = 3, UserId = 1, ClaimType = CoffeeClaims.CanViewCoffee, ClaimValue = "true" },
                new Claim { Id = 4, UserId = 2, ClaimType = CoffeeClaims.IsBarista, ClaimValue = "true" },
                new Claim { Id = 5, UserId = 2, ClaimType = CoffeeClaims.CanViewCoffee, ClaimValue = "true" }
            };

        modelBuilder.Entity<Claim>().HasData(adminClaims);

        // Seed some coffee data
        var coffees = new[]
        {
                new Coffee { Id = 1, Name = "Ethiopian Yirgacheffe", Description = "Bright and floral with citrus notes", Price = 18.99m, Origin = "Ethiopia", RoastLevel = "Light", IsAvailable = true, CreatedAt = DateTime.UtcNow },
                new Coffee { Id = 2, Name = "Colombian Supremo", Description = "Rich and balanced with chocolate undertones", Price = 16.99m, Origin = "Colombia", RoastLevel = "Medium", IsAvailable = true, CreatedAt = DateTime.UtcNow },
                new Coffee { Id = 3, Name = "Sumatra Mandheling", Description = "Full-bodied with earthy and herbal flavors", Price = 19.99m, Origin = "Indonesia", RoastLevel = "Dark", IsAvailable = true, CreatedAt = DateTime.UtcNow }
            };

        modelBuilder.Entity<Coffee>().HasData(coffees);
    }
}