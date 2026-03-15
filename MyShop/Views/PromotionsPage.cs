using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

namespace MyShop.Views;

public sealed class PromotionsPage : Page
{
    public PromotionsPage()
    {
        Content = new TextBlock
        {
            Text = "Promotions",
            Margin = new Thickness(16)
        };
    }
}
