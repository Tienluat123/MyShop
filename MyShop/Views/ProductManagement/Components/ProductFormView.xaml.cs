using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;

namespace MyShop.Views.ProductManagement.Components;

public sealed partial class ProductFormView : ContentDialog
{
    public ProductFormViewModel ViewModel => DataContext as ProductFormViewModel;

    public ProductFormView(ProductFormViewModel viewModel)
    {
        this.InitializeComponent();
        this.DataContext = viewModel;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel?.CancelCommand != null && ViewModel.CancelCommand.CanExecute(null))
            ViewModel.CancelCommand.Execute(null);

        this.Hide();
    }

    private void SubmitButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel?.SubmitCommand != null && ViewModel.SubmitCommand.CanExecute(null))
            ViewModel.SubmitCommand.Execute(null);

        this.Hide();
    }
}