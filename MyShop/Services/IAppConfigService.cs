using MyShop.Models;

namespace MyShop.Services;

/// <summary>
/// Interface for App Config Service — manages key-value application settings.
/// Implementations should cache values to avoid repeated DB reads.
/// </summary>
public interface IAppConfigService
{
    /// <summary>
    /// Gets a config value by its key. Returns null if not found.
    /// </summary>
    Task<string?> GetAsync(string key);

    /// <summary>
    /// Gets a config value by its key, returning a default if not found.
    /// </summary>
    Task<string> GetAsync(string key, string defaultValue);

    /// <summary>
    /// Sets a config value (insert or update).
    /// </summary>
    Task SetAsync(string key, string value);

    /// <summary>
    /// Gets all config entries as a dictionary.
    /// </summary>
    Task<Dictionary<string, string>> GetAllAsync();

    /// <summary>
    /// Gets the configured items per page for pagination. Defaults to 10.
    /// </summary>
    Task<int> GetPageSizeAsync();

    /// <summary>
    /// Reloads the in-memory cache from the database.
    /// </summary>
    Task ReloadCacheAsync();
}
