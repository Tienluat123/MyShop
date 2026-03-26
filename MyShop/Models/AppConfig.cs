namespace MyShop.Models;

public class AppConfig
{
    public int Id { get; set; }
    public string SettingKey { get; set; } = string.Empty; // VD: BackupPath, LogLevel, ShopName
    public string SettingValue { get; set; } = string.Empty;
    public string? Description { get; set; }
}