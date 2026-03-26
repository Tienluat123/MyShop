using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.ViewModels;
using MyShop.Views.ProductManagement.Components; // Nhớ thêm namespace này
using System;

namespace MyShop.Views.ProductManagement.Components;

public sealed partial class ProductManagementView : Page
{
    // Tạo property để gọi ViewModel cho lẹ
    public ProductManagementViewModel ViewModel => DataContext as ProductManagementViewModel;

    public ProductManagementView()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        DataContext = e.Parameter as ProductManagementViewModel;
    }

    // --- KHÚC SỬA QUAN TRỌNG Ở ĐÂY ---

    // Hàm hiện Dialog để thêm sản phẩm mới
    private async void AddProduct_Click(object sender, RoutedEventArgs e)
    {
        // Truyền đủ 3 tham số: isEditMode = false, item = null, callback = LoadData
        var formVM = new ProductFormViewModel(false, null, () =>
        {
        });

        var dialog = new ProductFormView(formVM);
        dialog.XamlRoot = this.XamlRoot;
        await dialog.ShowAsync();
    }

    // Hàm hiện Dialog để sửa sản phẩm đã chọn
    private async void EditProduct_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var selectedProduct = button?.CommandParameter as ProductListEntry;

        if (selectedProduct != null)
        {
            // Truyền đủ 3 tham số: isEditMode = true, item = selectedProduct, callback
            var formVM = new ProductFormViewModel(true, selectedProduct, () =>
            {
            });

            var dialog = new ProductFormView(formVM);
            dialog.XamlRoot = this.XamlRoot;
            await dialog.ShowAsync();
        }
    }
}