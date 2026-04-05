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
    if (this.DataContext is CustomerFormViewModel vm)
    {
        if (vm.CancelCommand.CanExecute(null)) vm.CancelCommand.Execute(null);
    }
    this.Hide();
}

private void ModalFooter_SubmitClicked(object sender, RoutedEventArgs e)
{
    if (this.DataContext is CustomerFormViewModel vm)
    {
        if (vm.SubmitCommand.CanExecute(null)) vm.SubmitCommand.Execute(null);
    }
    // Lưu ý: Đừng Hide ở đây nếu bạn muốn đợi Validate xong. 
    // Tốt nhất là để ViewModel gọi Hide thông qua onSuccess.
}}
