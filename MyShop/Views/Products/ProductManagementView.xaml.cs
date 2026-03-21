using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.ViewModels;

namespace MyShop.Views.Products;

public sealed partial class ProductManagementView : Page
{
    public ProductManagementView()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        DataContext = e.Parameter as ProductManagementViewModel;
    }
}
