using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MyShop.Models;
using MyShop.Services;

namespace MyShop.Data;

/// <summary>
/// Factory để tạo AppDbContext tại design-time
/// Được EF Core sử dụng khi chạy migrations
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    private readonly IConfigService _configService;

    public AppDbContextFactory() : this(new ConfigService())
    {
    }

    public AppDbContextFactory(IConfigService configService)
    {
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
    }

    public AppDbContext CreateDbContext(string[] args)
    {
        DbConfig dbConfig;
        try
        {
            dbConfig = _configService.LoadAsync().GetAwaiter().GetResult();
        }
        catch
        {
            dbConfig = new DbConfig();
        }

        var connectionString = dbConfig.ToConnectionString();

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

    /// <summary>
    /// Tạo DbContext mới cho runtime (không cần string[] args)
    /// </summary>
    public AppDbContext CreateDbContext()
    {
        return CreateDbContext(Array.Empty<string>());
    }
}
