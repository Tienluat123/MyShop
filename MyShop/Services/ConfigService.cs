using System.Text;
using System.Text.Json;
using MyShop.Models;
using Npgsql;

namespace MyShop.Services;

public interface IConfigService
{
    Task<DbConfig> LoadAsync();
    Task SaveAsync(DbConfig config);
    string GetConfigFilePath();
    bool HasSavedConfig();
    Task<(bool Success, string ErrorMessage)> TestConnectionAsync(DbConfig config);
    Task<(bool Success, string ErrorMessage)> TestSavedConfigAsync();
}

public class ConfigService : IConfigService
{
    private const string FileName = "dbconfig.json";
    private const string EncryptKey = "MyShop.DbConfig.Key.v1";

    private DbConfig? _cachedConfig; // Biến tạm lưu trên RAM

    public async Task<DbConfig> LoadAsync()
    {
        // Nếu đã có trong RAM rồi thì bốc ra xài luôn, không đọc ổ cứng nữa
        if (_cachedConfig != null) return _cachedConfig;

        try
        {
            var path = GetConfigFilePath();
            if (!File.Exists(path)) return new DbConfig();

            var json = await File.ReadAllTextAsync(path);
            if (string.IsNullOrWhiteSpace(json)) return new DbConfig();

            var fileModel = JsonSerializer.Deserialize<DbConfigFileModel>(json);
            if (fileModel == null) return new DbConfig();

            // Lưu vào Cache sau khi đọc thành công
            _cachedConfig = new DbConfig
            {
                Server = fileModel.Server ?? "localhost",
                Port = fileModel.Port <= 0 ? 5432 : fileModel.Port,
                DatabaseName = fileModel.DatabaseName ?? "myshop",
                Username = fileModel.Username ?? "postgres",
                Password = Decrypt(fileModel.PasswordEncrypted)
            };

            return _cachedConfig;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($" LoadConfig Error: {ex.Message}");
            return new DbConfig();
        }
    }

    public async Task SaveAsync(DbConfig config)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        var directory = Path.GetDirectoryName(GetConfigFilePath());
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var fileModel = new DbConfigFileModel
        {
            Server = config.Server,
            Port = config.Port,
            DatabaseName = config.DatabaseName,
            Username = config.Username,
            PasswordEncrypted = Encrypt(config.Password)
        };

        var json = JsonSerializer.Serialize(fileModel, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(GetConfigFilePath(), json);
        _cachedConfig = config; // Cập nhật luôn vào Cache sau khi lưu thành công
    }

    public string GetConfigFilePath()
    {
        try
        {
            var localFolder = global::Windows.Storage.ApplicationData.Current.LocalFolder.Path;
            return Path.Combine(localFolder, FileName);
        }
        catch
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(appData, "MyShop", FileName);
        }
    }

    public bool HasSavedConfig()
    {
        return File.Exists(GetConfigFilePath());
    }

    public async Task<(bool Success, string ErrorMessage)> TestConnectionAsync(DbConfig config)
    {
        if (config == null)
            return (false, "Cấu hình database không hợp lệ.");

        try
        {
            await using var connection = new NpgsqlConnection(config.ToConnectionString());
            await connection.OpenAsync();
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string ErrorMessage)> TestSavedConfigAsync()
    {
        if (!HasSavedConfig())
            return (false, "Chưa có cấu hình database.");

        var config = await LoadAsync();
        return await TestConnectionAsync(config);
    }

    private static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        var data = Encoding.UTF8.GetBytes(plainText);
        var key = Encoding.UTF8.GetBytes(EncryptKey);

        for (int i = 0; i < data.Length; i++)
            data[i] = (byte)(data[i] ^ key[i % key.Length]);

        return Convert.ToBase64String(data);
    }

    private static string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return string.Empty;

        try
        {
            var data = Convert.FromBase64String(cipherText);
            var key = Encoding.UTF8.GetBytes(EncryptKey);

            for (int i = 0; i < data.Length; i++)
                data[i] = (byte)(data[i] ^ key[i % key.Length]);

            return Encoding.UTF8.GetString(data);
        }
        catch
        {
            return string.Empty;
        }
    }

    private sealed class DbConfigFileModel
    {
        public string Server { get; set; } = "localhost";
        public int Port { get; set; } = 5432;
        public string DatabaseName { get; set; } = "myshop";
        public string Username { get; set; } = "postgres";
        public string PasswordEncrypted { get; set; } = string.Empty;
    }
}
