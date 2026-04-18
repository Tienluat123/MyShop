using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;

namespace MyShop.Views.Promotion.Components;

public sealed partial class AddPromotionModal : ContentDialog
{
    public AddPromotionModal(PromotionFormViewModel viewModel)
    {
        this.InitializeComponent();
        this.DataContext = viewModel;
    }

    private void CancelButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (this.DataContext is PromotionFormViewModel vm && vm.CancelCommand.CanExecute(null))
        {
            vm.CancelCommand.Execute(null);
        }
        this.Hide();
    }

    private void SaveButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (this.DataContext is PromotionFormViewModel vm)
        {
            if (vm.SubmitCommand.CanExecute(null))
            {
                vm.SubmitCommand.Execute(null);
            }

            // Chỉ đóng dialog khi KHÔNG có lỗi validation và KHÔNG có lỗi hệ thống
            if (vm.ValidationErrors.Count == 0 && string.IsNullOrEmpty(vm.ErrorMessage))
            {
                this.Hide();
            }
            // Nếu có lỗi → giữ dialog mở để user thấy lỗi và sửa
        }
    }
}
