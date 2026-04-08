using Microsoft.UI.Xaml.Controls;
using MyShop.Di;
using MyShop.Services;
using MyShop.ViewModels;
using MyShop.Views.ProductManagement.Components;
using System;
using System.Diagnostics.CodeAnalysis;

namespace MyShop.Views.ProductManagement;

public sealed partial class ManageProductPage : Page
{
    private readonly IProductService _productService;

    public ManageProductPage()
    {
        InitializeComponent();
        _productService = ServiceContainer.Instance.Get<IProductService>();
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
            productService: _productService,
            onAddProduct: () => ShowFormDialog(false),
            onEditProduct: product => ShowFormDialog(true, product));

        ProductContentFrame.Navigate(typeof(ProductManagementView), vm);
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    private async void ShowFormDialog(bool isEditMode, ProductListEntry? product = null)
    {
        var vm = new ProductFormViewModel(isEditMode, product, onCancel: () => { });
        var dialog = new ProductFormView(vm)
        {
            XamlRoot = this.XamlRoot
        };
        await dialog.ShowAsync();
    }
}
