using System;
using Microsoft.EntityFrameworkCore;
using MyShop.Models;

namespace MyShop.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Khai báo các bảng để Entity Framework nhận diện
    public DbSet<AppConfig> AppConfigs { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Promotion> Promotions { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderDetail> OrderDetails { get; set; } = null!;
    public DbSet<ProductImage> ProductImages { get; set; } = null!;
    public DbSet<LoyaltyTierConfig> LoyaltyTierConfigs { get; set; } = null!;
    public DbSet<UserSetting> UserSettings { get; set; } = null!;
    public DbSet<WorkSchedule> WorkSchedules { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ===== USER RELATIONSHIPS =====
        // User - Orders (1 to Many)
        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // User - WorkSchedules (1 to Many)
        modelBuilder.Entity<WorkSchedule>()
            .HasOne(ws => ws.User)
            .WithMany(u => u.WorkSchedules)
            .HasForeignKey(ws => ws.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // User - UserSettings (1 to Many)
        modelBuilder.Entity<UserSetting>()
            .HasOne(us => us.User)
            .WithMany(u => u.UserSettings)
            .HasForeignKey(us => us.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ===== CATEGORY RELATIONSHIPS =====
        // Category - Products (1 to Many)
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // ===== CUSTOMER RELATIONSHIPS =====
        // Customer - Orders (1 to Many)
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // ===== ORDER RELATIONSHIPS =====
        // Order - OrderDetails (1 to Many)
        modelBuilder.Entity<OrderDetail>()
            .HasOne(od => od.Order)
            .WithMany(o => o.OrderDetails)
            .HasForeignKey(od => od.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Order - Promotion (Many to One)
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Promotion)
            .WithMany(p => p.Orders)
            .HasForeignKey(o => o.PromotionId)
            .OnDelete(DeleteBehavior.SetNull);

        // ===== ORDER DETAIL RELATIONSHIPS =====
        // OrderDetail - Product (Many to One)
        modelBuilder.Entity<OrderDetail>()
            .HasOne(od => od.Product)
            .WithMany(p => p.OrderDetails)
            .HasForeignKey(od => od.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Product - ProductImages (1 to Many)
        modelBuilder.Entity<ProductImage>()
            .HasOne(pi => pi.Product)
            .WithMany(p => p.ProductImages)
            .HasForeignKey(pi => pi.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProductImage>()
            .Property(pi => pi.CreatedAt)
            .HasDefaultValueSql("NOW()");

        modelBuilder.Entity<ProductImage>()
            .ToTable(t => t.HasCheckConstraint("CK_ProductImages_SortOrder_1_3", "\"SortOrder\" >= 1 AND \"SortOrder\" <= 3"));

        modelBuilder.Entity<ProductImage>()
            .HasIndex(pi => new { pi.ProductId, pi.SortOrder })
            .IsUnique();

        modelBuilder.Entity<ProductImage>()
            .HasIndex(pi => pi.ProductId)
            .IsUnique()
            .HasFilter("\"IsPrimary\" = TRUE");

        // ===== UNIQUE CONSTRAINTS =====
        // PromoCode must be unique
        modelBuilder.Entity<Promotion>()
            .HasIndex(p => p.PromoCode)
            .IsUnique();

        // LoyaltyTierConfig.ConfigKey must be unique
        modelBuilder.Entity<LoyaltyTierConfig>()
            .HasIndex(c => c.ConfigKey)
            .IsUnique();

        // Username must be unique
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        // ===== PRECISION FOR DECIMAL FIELDS =====
        modelBuilder.Entity<Order>()
            .Property(o => o.SubTotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.DiscountAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrderDetail>()
            .Property(od => od.UnitPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<User>()
            .Property(u => u.HourlyRate)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Promotion>()
            .Property(p => p.DiscountValue)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Promotion>()
            .Property(p => p.MinOrderAmount)
            .HasPrecision(18, 2);

        // ===== DEFAULT VALUES =====
        modelBuilder.Entity<User>()
            .Property(u => u.IsActive)
            .HasDefaultValue(true);

        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasDefaultValue("2"); // Staff role

        modelBuilder.Entity<Promotion>()
            .Property(p => p.IsActive)
            .HasDefaultValue(true);

        modelBuilder.Entity<Order>()
            .Property(o => o.OrderDate)
            .HasDefaultValueSql("NOW()");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var conn = Environment.GetEnvironmentVariable("CONNECTION_STRING")
                       ?? Environment.GetEnvironmentVariable("DATABASE_URL");

            if (string.IsNullOrWhiteSpace(conn))
            {
                // Fallback placeholder - replace with a real connection or set env var
                conn = "Host=aws-0-...pooler.supabase.com;Database=postgres;Username=postgres.xxx;Password=MAT_KHAU_CUA_ONG";
            }

            optionsBuilder.UseNpgsql(conn);
        }
    }
}