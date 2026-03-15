using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

namespace MyShop.Views;

public sealed class OrdersPage : Page
{
    public OrdersPage()
    {
        Content = new TextBlock
        {
            Text = "Orders",
            Margin = new Thickness(16)
        };
    }
}
