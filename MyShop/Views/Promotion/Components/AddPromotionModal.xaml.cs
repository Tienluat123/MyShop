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
        if (this.DataContext is PromotionFormViewModel vm && vm.SubmitCommand.CanExecute(null))
        {
            vm.SubmitCommand.Execute(null);
        }

        this.Hide();
    }
}