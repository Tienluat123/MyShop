using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;
using System.ComponentModel;

namespace MyShop.Views.AppConfig;

public sealed partial class DatabaseConfigPage : Page
{
    public DatabaseConfigViewModel ViewModel { get; }
    public event EventHandler? ConfigurationSaved;
    public event EventHandler? BackToOnboardingRequested;

    public DatabaseConfigPage()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<DatabaseConfigViewModel>();
        DataContext = ViewModel;
        PasswordInput.Password = ViewModel.Password;
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
        ViewModel.ConfigSavedSuccessfully += OnConfigSavedSuccessfully;
        this.Unloaded += DatabaseConfigPage_Unloaded;
    }

    private void PasswordInput_PasswordChanged(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ViewModel.Password = PasswordInput.Password;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DatabaseConfigViewModel.Password) && PasswordInput.Password != ViewModel.Password)
        {
            PasswordInput.Password = ViewModel.Password;
        }
    }

    private void OnConfigSavedSuccessfully(object? sender, EventArgs e)
    {
        ConfigurationSaved?.Invoke(this, EventArgs.Empty);
    }

    private void BackToOnboardingButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        BackToOnboardingRequested?.Invoke(this, EventArgs.Empty);
    }

    private void DatabaseConfigPage_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        ViewModel.ConfigSavedSuccessfully -= OnConfigSavedSuccessfully;
        this.Unloaded -= DatabaseConfigPage_Unloaded;
    }
}
