using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using MyShop.Models;
using MyShop.Services;

namespace MyShop.ViewModels;

public partial class DatabaseConfigViewModel : ObservableObject
{
    private readonly IConfigService _configService;
    public event EventHandler? ConfigSavedSuccessfully;

    [ObservableProperty] private string server = "localhost";
    [ObservableProperty] private int port = 5432;
    [ObservableProperty] private string databaseName = "myshop";
    [ObservableProperty] private string username = "postgres";
    [ObservableProperty] private string password = string.Empty;

    [ObservableProperty] private bool isTestingConnection;
    [ObservableProperty] private bool isInfoBarOpen;
    [ObservableProperty] private string infoMessage = string.Empty;
    [ObservableProperty] private string infoTitle = "Thông báo";
    [ObservableProperty] private InfoBarSeverity infoSeverity = InfoBarSeverity.Informational;

    public DatabaseConfigViewModel(IConfigService configService)
    {
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _ = LoadConfigAsync();
    }

    [RelayCommand]
    private async Task LoadConfigAsync()
    {
        var config = await _configService.LoadAsync();
        Server = config.Server;
        Port = config.Port;
        DatabaseName = config.DatabaseName;
        Username = config.Username;
        Password = config.Password;
    }

    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        if (IsTestingConnection)
            return;

        try
        {
            IsTestingConnection = true;
            IsInfoBarOpen = false;

            var config = BuildCurrentConfig();
            var result = await _configService.TestConnectionAsync(config);

            if (!result.Success)
            {
                InfoTitle = "Thất bại";
                InfoMessage = $"Không thể kết nối: {result.ErrorMessage}";
                InfoSeverity = InfoBarSeverity.Error;
                IsInfoBarOpen = true;
                return;
            }

            InfoTitle = "Thành công";
            InfoMessage = "Kết nối cơ sở dữ liệu thành công.";
            InfoSeverity = InfoBarSeverity.Success;
            IsInfoBarOpen = true;
        }
        catch (Exception ex)
        {
            InfoTitle = "Thất bại";
            InfoMessage = $"Không thể kết nối: {ex.Message}";
            InfoSeverity = InfoBarSeverity.Error;
            IsInfoBarOpen = true;
        }
        finally
        {
            IsTestingConnection = false;
        }
    }

    [RelayCommand]
    private async Task SaveConfigAsync()
    {
        try
        {
            var config = BuildCurrentConfig();
            await _configService.SaveAsync(config);

            InfoTitle = "Đã lưu";
            InfoMessage = "Đã lưu cấu hình";
            InfoSeverity = InfoBarSeverity.Warning;
            IsInfoBarOpen = true;
            ConfigSavedSuccessfully?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            InfoTitle = "Lỗi";
            InfoMessage = $"Không thể lưu cấu hình: {ex.Message}";
            InfoSeverity = InfoBarSeverity.Error;
            IsInfoBarOpen = true;
        }
    }

    private DbConfig BuildCurrentConfig()
    {
        return new DbConfig
        {
            Server = string.IsNullOrWhiteSpace(Server) ? "localhost" : Server.Trim(),
            Port = Port <= 0 ? 5432 : Port,
            DatabaseName = string.IsNullOrWhiteSpace(DatabaseName) ? "myshop" : DatabaseName.Trim(),
            Username = string.IsNullOrWhiteSpace(Username) ? "postgres" : Username.Trim(),
            Password = Password ?? string.Empty
        };
    }
}
