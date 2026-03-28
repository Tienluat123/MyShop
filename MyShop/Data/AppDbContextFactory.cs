using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MyShop.Data;

/// <summary>
/// Factory để tạo AppDbContext tại design-time
/// Được EF Core sử dụng khi chạy migrations
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Supabase PostgreSQL connection (development)
        var dbConfig = new DatabaseConfig
        {
            Host = "aws-1-ap-northeast-1.pooler.supabase.com",
            Database = "postgres",
            Username = "postgres.oivqtyqiwschphohiblr",
            Password = "laptrinhwindow",
            Port = 5432,
            SslMode = "Require"
        };

        var connectionString = dbConfig.GetConnectionString();

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString, options =>
        {
            options.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null
            );
        });

        return new AppDbContext(optionsBuilder.Options);
    }
}
