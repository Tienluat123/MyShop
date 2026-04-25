using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using MyShop.ViewModels;

namespace MyShop.Views.ProductManagement.Components;

public sealed partial class ProductManagementView : Page
{
    public ProductManagementViewModel? ViewModel => DataContext as ProductManagementViewModel;

    public ProductManagementView()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        DataContext = e.Parameter as ProductManagementViewModel;
    }

    private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox comboBox)
        {
            return;
        }

        if (comboBox.SelectedValue is string sortKey &&
            ViewModel?.SortProductsCommand != null &&
            ViewModel.SortProductsCommand.CanExecute(sortKey))
        {
            ViewModel.SortProductsCommand.Execute(sortKey);
        }
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element || element.DataContext is not ProductListEntry product)
        {
            return;
        }

        var dialog = new ContentDialog
        {
            XamlRoot = this.XamlRoot,
            Title = "Confirm Delete",
            Content = $"Delete product '{product.Name}'?",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
        {
            return;
        }

        if (ViewModel?.DeleteProductCommand != null && ViewModel.DeleteProductCommand.CanExecute(product))
        {
            ViewModel.DeleteProductCommand.Execute(product);
        }
    }

    private async void ImportButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel == null)
        {
            return;
        }

        var pathTextBox = new TextBox
        {
            PlaceholderText = "D:\\data\\catalog-phase1.xlsx"
        };

        var dialog = new ContentDialog
        {
            XamlRoot = this.XamlRoot,
            Title = "Import Product Catalog",
            Content = pathTextBox,
            PrimaryButtonText = "Import",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
        {
            return;
        }

        var filePath = pathTextBox.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(filePath))
        {
            ViewModel.ErrorMessage = "Please provide a valid .xlsx file path.";
            return;
        }

        await ViewModel.ImportFromExcelCommand.ExecuteAsync(filePath);
    }
}