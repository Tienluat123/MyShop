using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;
using MyShop.Models;

namespace MyShop.Views.Pos.Components;

public sealed partial class ProductCustomizationModal : ContentDialog
{
    public ProductCustomizationViewModel ViewModel;

    public ProductCustomizationModal(Product product, OrderItem? existingItem = null)
    {
        this.InitializeComponent();
        Loaded += ProductCustomizationModal_Loaded;
        ViewModel = new ProductCustomizationViewModel(product, existingItem);
        DataContext = ViewModel;

        // Clear default title as we use custom header
        Title = "";

        if (product.Category?.Name != "Coffee" && product.Category?.Name != "Tea")
        {
            BeverageOptions.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        }

        //ViewModel.DrinkConfirmed += (opts, qty) => this.Hide();
    }

    private void ProductCustomizationModal_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ProductCustomizationViewModel vm)
        {
            vm.ItemConfirmed += item => Hide();
        }
    }

    private void CloseBtn_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        this.Hide();
    }
}
