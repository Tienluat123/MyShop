using System;
using Microsoft.EntityFrameworkCore;
using MyShop.Models; // Đổi lại namespace theo project của ông

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