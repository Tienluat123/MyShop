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
private void TextBox_LostFocus_Name(object sender, RoutedEventArgs e)
    {
        if (DataContext is CustomerFormViewModel vm) vm.ValidateNameCommand.Execute(null);
    }

    private void TextBox_LostFocus_Phone(object sender, RoutedEventArgs e)
    {
        if (DataContext is CustomerFormViewModel vm) vm.ValidatePhoneCommand.Execute(null);
    }

    private void TextBox_LostFocus_Email(object sender, RoutedEventArgs e)
    {
        if (DataContext is CustomerFormViewModel vm) vm.ValidateEmailCommand.Execute(null);
    }
    private void ModalFooter_CancelClicked(object sender, RoutedEventArgs e)
    {
    
        this.Hide();
    }

   private async void ModalFooter_SubmitClicked(object sender, RoutedEventArgs e)
    {
        if (this.DataContext is CustomerFormViewModel vm)
        {
            // Gọi lệnh Submit trong ViewModel và đợi nó chạy xong
            await vm.SubmitCommand.ExecuteAsync(null);

            // Kiểm tra: Nếu không còn lỗi (Validate sạch sẽ) thì mới đóng Dialog
            if (string.IsNullOrEmpty(vm.NameError) && 
                string.IsNullOrEmpty(vm.PhoneError) && 
                string.IsNullOrEmpty(vm.EmailError))
            {
                this.Hide();
            }
        }
    }
}
