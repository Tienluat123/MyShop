using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MyShop.Views.AppConfig;

public sealed partial class AppConfigPage : Page
{
    public AppConfigPage()
    {
        this.InitializeComponent();
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Save Successful",
            Content = "System settings have been saved successfully!",
            CloseButtonText = "Close",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.XamlRoot
        };
        await dialog.ShowAsync();
    }
}
