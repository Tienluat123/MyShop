using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

namespace MyShop.Views;

public sealed class CustomersPage : Page
{
    public CustomersPage()
    {
        Content = new TextBlock
        {
            Text = "Customers",
            Margin = new Thickness(16)
        };
    }
}
