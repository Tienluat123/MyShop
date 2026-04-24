using System.Collections.Concurrent;
using MyShop.Data;
using MyShop.Models;

namespace MyShop.Services;

/// <summary>
/// Singleton service that manages application configuration stored in AppConfigs table.
/// Values are cached in-memory to avoid repeated DB reads.
/// </summary>
public class AppConfigService : IAppConfigService
{
    private readonly IRepository<AppConfig> _repository;
    private ConcurrentDictionary<string, string> _cache = new();

    public AppConfigService(IRepository<AppConfig> repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<string?> GetAsync(string key)
    {
        if (_cache.TryGetValue(key, out var cached))
            return cached;

        var all = await _repository.GetAllAsync();
        var entry = all.Items.FirstOrDefault(c => c.SettingKey == key);
        if (entry == null) return null;

        _cache[key] = entry.SettingValue;
        return entry.SettingValue;
    }

    public async Task<string> GetAsync(string key, string defaultValue)
    {
        return await GetAsync(key) ?? defaultValue;
    }

    public async Task SetAsync(string key, string value)
    {
        var all = await _repository.GetAllAsync();
        var existing = all.Items.FirstOrDefault(c => c.SettingKey == key);

        if (existing != null)
        {
            existing.SettingValue = value;
            await _repository.UpdateAsync(existing);
        }
        else
        {
            var newEntry = new AppConfig
            {
                SettingKey = key,
                SettingValue = value
            };
            await _repository.AddAsync(newEntry);
        }

        // Update cache
        _cache[key] = value;
    }

    public async Task<Dictionary<string, string>> GetAllAsync()
    {
        var all = await _repository.GetAllAsync();
        return all.Items.ToDictionary(c => c.SettingKey, c => c.SettingValue);
    }

    public async Task<int> GetPageSizeAsync()
    {
        var value = await GetAsync("ItemsPerPage", "10");
        if (int.TryParse(value, out var pageSize))
        {
            return pageSize;
        }
        return 10;
    }

    public async Task ReloadCacheAsync()
    {
        _cache.Clear();
        var all = await GetAllAsync();
        foreach (var kvp in all)
            _cache[kvp.Key] = kvp.Value;
    }
}
