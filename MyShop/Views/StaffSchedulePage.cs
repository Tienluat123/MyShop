using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

namespace MyShop.Views;

public sealed class StaffSchedulePage : Page
{
    public StaffSchedulePage()
    {
        Content = new TextBlock
        {
            Text = "Staff Schedule",
            Margin = new Thickness(16)
        };
    }
}
