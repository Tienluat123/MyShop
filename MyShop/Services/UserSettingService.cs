using Microsoft.EntityFrameworkCore;
using MyShop.Data;
using MyShop.Models;

namespace MyShop.Services;

public class UserSettingService : IUserSettingService
{
    private readonly IRepository<UserSetting> _repository;

    public UserSettingService(IRepository<UserSetting> repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<string> GetAsync(int userId, string key, string defaultValue)
    {
        var setting = await _repository.ExecuteAsync(async context =>
        {
            return await context.UserSettings
                .AsNoTracking()
                .Where(s => s.UserId == userId && s.SettingKey == key)
                .OrderBy(s => s.Id)
                .FirstOrDefaultAsync();
        }, "GetUserSettingAsync");

        return setting?.SettingValue ?? defaultValue;
    }

    public async Task SetAsync(int userId, string key, string value)
    {
        await _repository.ExecuteAsync(async context =>
        {
            var settings = await context.UserSettings
                .Where(s => s.UserId == userId && s.SettingKey == key)
                .OrderBy(s => s.Id)
                .ToListAsync();

            var existing = settings.FirstOrDefault();
            if (existing == null)
            {
                context.UserSettings.Add(new UserSetting
                {
                    UserId = userId,
                    SettingKey = key,
                    SettingValue = value
                });
            }
            else
            {
                existing.SettingValue = value;

                foreach (var duplicate in settings.Skip(1))
                {
                    context.UserSettings.Remove(duplicate);
                }
            }

            await context.SaveChangesAsync();
            return true;
        }, "SetUserSettingAsync");
    }
}
