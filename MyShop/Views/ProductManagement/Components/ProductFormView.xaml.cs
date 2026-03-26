using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;

namespace MyShop.Views.ProductManagement.Components;

public sealed partial class ProductFormView : ContentDialog
{
    public ProductFormViewModel ViewModel => DataContext as ProductFormViewModel;

    // 2. Sửa hàm khởi tạo để nhận ViewModel truyền vào
    public ProductFormView(ProductFormViewModel viewModel)
    {
        this.InitializeComponent();
        this.DataContext = viewModel;
    }

    // 3. Các hàm hứng sự kiện từ ModalHeader/Footer (phải có để nút bấm chạy)
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel?.CancelCommand != null && ViewModel.CancelCommand.CanExecute(null))
            ViewModel.CancelCommand.Execute(null);

        this.Hide(); // Đóng Dialog
    }

    private void SubmitButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel?.SubmitCommand != null && ViewModel.SubmitCommand.CanExecute(null))
            ViewModel.SubmitCommand.Execute(null);

        this.Hide(); // Đóng Dialog
    }
}