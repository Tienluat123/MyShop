using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.ViewModels;

namespace MyShop.Views.CustomerManagement.Components;

public sealed partial class CustomerFormView : ContentDialog
{
    public CustomerFormView(CustomerFormViewModel viewModel)
    {
        this.InitializeComponent();
        // Gắn DataContext ngay tại đây luôn
        this.DataContext = viewModel;
    }

    private void ModalFooter_CancelClicked(object sender, RoutedEventArgs e)
    {
        // Nếu ViewModel có logic gì cần chạy khi Hủy thì gọi ở đây
        if (this.DataContext is CustomerFormViewModel vm && vm.CancelCommand.CanExecute(null))
        {
            vm.CancelCommand.Execute(null);
        }

        // Lệnh quan trọng nhất: ĐÓNG CÁI DIALOG LẠI
        this.Hide();
    }

    private void ModalFooter_SubmitClicked(object sender, RoutedEventArgs e)
    {
        // Gọi lệnh Lưu (Submit) bên trong ViewModel
        if (this.DataContext is CustomerFormViewModel vm && vm.SubmitCommand.CanExecute(null))
        {
            vm.SubmitCommand.Execute(null);
        }

        // Tùy logic của sếp: Lưu xong thì đóng Dialog lại
        this.Hide();
    }
}
