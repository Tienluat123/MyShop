using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

namespace MyShop.Views;

public sealed class AppConfigPage : Page
{
    public AppConfigPage()
    {
        Content = new TextBlock
        {
            Text = "App Config",
            Margin = new Thickness(16)
        };
    }
}
