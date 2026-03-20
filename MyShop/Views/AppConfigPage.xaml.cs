using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MyShop.Views;

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
            Title = "Lưu thành công",
            Content = "Cài đặt hệ thống đã được lưu thành công!",
            CloseButtonText = "Đóng",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.XamlRoot
        };
        await dialog.ShowAsync();
    }
}
