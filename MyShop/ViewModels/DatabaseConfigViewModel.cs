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
    [ObservableProperty] private string infoTitle = "Notice";
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
                InfoTitle = "Failed";
                InfoMessage = $"Unable to connect: {result.ErrorMessage}";
                InfoSeverity = InfoBarSeverity.Error;
                IsInfoBarOpen = true;
                return;
            }

            InfoTitle = "Success";
            InfoMessage = "Database connection succeeded.";
            InfoSeverity = InfoBarSeverity.Success;
            IsInfoBarOpen = true;
        }
        catch (Exception ex)
        {
            InfoTitle = "Failed";
            InfoMessage = $"Unable to connect: {ex.Message}";
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

            InfoTitle = "Saved";
            InfoMessage = "Configuration saved.";
            InfoSeverity = InfoBarSeverity.Warning;
            IsInfoBarOpen = true;
            ConfigSavedSuccessfully?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            InfoTitle = "Error";
            InfoMessage = $"Unable to save configuration: {ex.Message}";
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
