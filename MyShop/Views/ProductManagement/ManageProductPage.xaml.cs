using Microsoft.UI.Xaml.Controls;
using MyShop.Di;
using MyShop.Services;
using MyShop.Services.ExcelImport;
using MyShop.ViewModels;
using MyShop.Views.ProductManagement.Components;
using System;
using System.Diagnostics.CodeAnalysis;

namespace MyShop.Views.ProductManagement;

public sealed partial class ManageProductPage : Page
{
    private readonly IProductService _productService;
    private readonly IExcelCatalogImportService _excelCatalogImportService;
    private readonly INotificationService _notificationService;

    public ManageProductPage()
    {
        InitializeComponent();
        _productService = ServiceContainer.Instance.Get<IProductService>();
        _excelCatalogImportService = ServiceContainer.Instance.Get<IExcelCatalogImportService>();
        _notificationService = ServiceContainer.Instance.Get<INotificationService>();
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
            excelCatalogImportService: _excelCatalogImportService,
            notificationService: _notificationService,
            onAddProduct: () => ShowFormDialog(false),
            onEditProduct: product => ShowFormDialog(true, product));

        ProductContentFrame.Navigate(typeof(ProductManagementView), vm);
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    private async void ShowFormDialog(bool isEditMode, ProductListEntry? product = null)
    {
        var managementVm = (ProductContentFrame.Content as ProductManagementView)?.ViewModel;
        var vm = new ProductFormViewModel(
            isEditMode,
            product,
            onCancel: () => { },
            onSaved: () => managementVm?.LoadProductsCommand.Execute(null));
        var dialog = new ProductFormView(vm)
        {
            XamlRoot = this.XamlRoot
        };
        await dialog.ShowAsync();
    }
}
