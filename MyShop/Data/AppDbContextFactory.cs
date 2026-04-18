using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Text.Json;

namespace MyShop.Data;

/// <summary>
/// Factory để tạo AppDbContext tại design-time
/// Được EF Core sử dụng khi chạy migrations
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Try to read connection string from appsettings.json
        var dbConfig = TryReadAppsettingsConfig()
            ?? new DatabaseConfig
            {
                Host = "localhost",
                Database = "myshop",
                Username = "nguyentienluat",
                Password = string.Empty,
                Port = 5432,
                SslMode = "Disable"
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

    private DatabaseConfig? TryReadAppsettingsConfig()
    {
        try
        {
            // Find appsettings.json in current or parent directory
            var paths = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "MyShop", "appsettings.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "appsettings.json")
            };

            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("Database", out var dbElement))
                    {
                        return new DatabaseConfig
                        {
                            Host = dbElement.GetProperty("Host").GetString() ?? "localhost",
                            Database = dbElement.GetProperty("Database").GetString() ?? "myshop",
                            Username = dbElement.GetProperty("Username").GetString() ?? "nguyentienluat",
                            Password = dbElement.GetProperty("Password").GetString() ?? string.Empty,
                            Port = dbElement.GetProperty("Port").GetInt32(),
                            SslMode = dbElement.GetProperty("SslMode").GetString() ?? "Disable"
                        };
                    }
                    break;
                }
            }
        }
        catch
        {
            // Ignore errors, use default
        }

        return null;
    }

    /// <summary>
    /// Tạo DbContext mới cho runtime (không cần string[] args)
    /// </summary>
    public AppDbContext CreateDbContext()
    {
        return CreateDbContext(Array.Empty<string>());
    }
}
