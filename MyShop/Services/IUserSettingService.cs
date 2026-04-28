namespace MyShop.Services;

public interface IUserSettingService
{
    Task<string> GetAsync(int userId, string key, string defaultValue);
    Task SetAsync(int userId, string key, string value);
}
