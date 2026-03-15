using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

namespace MyShop.Views;

public sealed class ManageProductPage : Page
{
    public ManageProductPage()
    {
        Content = new TextBlock
        {
            Text = "Manage Products",
            Margin = new Thickness(16)
        };
    }
}
