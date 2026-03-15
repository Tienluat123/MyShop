using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

namespace MyShop.Views;

public sealed class PosPage : Page
{
    public PosPage()
    {
        Content = new TextBlock
        {
            Text = "POS",
            Margin = new Thickness(16)
        };
    }
}
