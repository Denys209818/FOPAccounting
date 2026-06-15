using FopAccounting.Models;
using Microsoft.EntityFrameworkCore;

namespace FopAccounting.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<BusinessProfile> BusinessProfiles => Set<BusinessProfile>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserProfileAccess> UserProfileAccesses => Set<UserProfileAccess>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockOperation> StockOperations => Set<StockOperation>();
    public DbSet<ManagementOperation> ManagementOperations => Set<ManagementOperation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<BusinessProfile>().ToTable("BusinessProfiles");
        modelBuilder.Entity<Role>().ToTable("Roles");
        modelBuilder.Entity<UserProfileAccess>().ToTable("UserProfileAccess");
        modelBuilder.Entity<Product>().ToTable("Products");
        modelBuilder.Entity<StockOperation>().ToTable("StockOperations");
        modelBuilder.Entity<ManagementOperation>().ToTable("ManagementOperations");

        modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();

        modelBuilder.Entity<BusinessProfile>()
            .HasOne(x => x.Owner)
            .WithMany(x => x.BusinessProfiles)
            .HasForeignKey(x => x.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserProfileAccess>()
            .HasOne(x => x.User)
            .WithMany(x => x.Accesses)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserProfileAccess>()
            .HasOne(x => x.Profile)
            .WithMany(x => x.Accesses)
            .HasForeignKey(x => x.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserProfileAccess>()
            .HasOne(x => x.Role)
            .WithMany(x => x.Accesses)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Product>()
            .Property(x => x.PurchasePrice)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Product>()
            .Property(x => x.SalePrice)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Product>()
            .HasOne(x => x.Profile)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StockOperation>()
            .HasOne(x => x.Product)
            .WithMany(x => x.StockOperations)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ManagementOperation>()
            .Property(x => x.Amount)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<ManagementOperation>()
            .HasOne(x => x.Profile)
            .WithMany(x => x.ManagementOperations)
            .HasForeignKey(x => x.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
