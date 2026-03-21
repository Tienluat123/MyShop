using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;
using MyShop.Views.ProductManagement.Components;
using System.Diagnostics.CodeAnalysis;

namespace MyShop.Views.ProductManagement;

public sealed partial class ManageProductPage : Page
{
    public ManageProductPage()
    {
        InitializeComponent();
        Loaded += ManageProductPage_Loaded;
    }

    private void ManageProductPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (ProductContentFrame.Content is null)
        {
            NavigateToManagement();
        }
    }

    private void NavigateToManagement()
    {
        var vm = new ProductManagementViewModel(
            onAddProduct: () => NavigateToForm(false),
            onEditProduct: product => NavigateToForm(true, product));

        ProductContentFrame.Navigate(typeof(ProductManagementView), vm);
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    private void NavigateToForm(bool isEditMode, ProductListEntry? product = null)
    {
        var vm = new ProductFormViewModel(isEditMode, product, onCancel: NavigateToManagement);
        ProductContentFrame.Navigate(typeof(ProductFormView), vm);
    }
}
