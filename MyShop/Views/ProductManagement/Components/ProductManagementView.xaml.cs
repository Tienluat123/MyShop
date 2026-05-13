using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using System.IO;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;
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

    private void PreviousButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ViewModel?.PreviousPageCommand.Execute(null);
    }

    private void NextButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ViewModel?.NextPageCommand.Execute(null);
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

        try
        {
            var filePath = await PickExcelFileAsync();
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return;
            }

            await ViewModel.ImportFromExcelCommand.ExecuteAsync(filePath);
        }
        catch (Exception ex)
        {
            ViewModel.ErrorMessage = $"Failed to start Excel import: {ex.Message}";
        }
    }

    private async Task<string?> PickExcelFileAsync()
    {
        string? pickerError = null;

        try
        {
            if (Application.Current is MyShop.App app && app.MainWindow != null)
            {
                var picker = new FileOpenPicker
                {
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary
                };

                picker.FileTypeFilter.Add(".xlsx");

                InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(app.MainWindow));

                var file = await picker.PickSingleFileAsync();
                var resolvedPath = await ResolvePickedFilePathAsync(file);
                if (!string.IsNullOrWhiteSpace(resolvedPath))
                {
                    return resolvedPath;
                }

                if (file != null)
                {
                    pickerError = "The selected Excel file did not expose a readable local path. Enter the file path manually.";
                }
            }
        }
        catch (Exception ex)
        {
            pickerError = $"The file picker could not be opened: {ex.Message}";
        }

        return await PromptForExcelPathAsync(pickerError);
    }

    private static async Task<string?> ResolvePickedFilePathAsync(StorageFile? file)
    {
        if (file == null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(file.Path) && File.Exists(file.Path))
        {
            return file.Path;
        }

        try
        {
            var copiedFile = await file.CopyAsync(
                ApplicationData.Current.TemporaryFolder,
                file.Name,
                NameCollisionOption.GenerateUniqueName);

            if (!string.IsNullOrWhiteSpace(copiedFile.Path) && File.Exists(copiedFile.Path))
            {
                return copiedFile.Path;
            }
        }
        catch
        {
        }

        return null;
    }

    private async Task<string?> PromptForExcelPathAsync(string? pickerError)
    {
        var content = new StackPanel
        {
            Spacing = 10
        };

        if (!string.IsNullOrWhiteSpace(pickerError))
        {
            content.Children.Add(new TextBlock
            {
                Text = pickerError,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 146, 64, 14))
            });
        }

        var pathTextBox = new TextBox
        {
            PlaceholderText = @"D:\data\catalog-phase1.xlsx"
        };

        content.Children.Add(pathTextBox);

        var dialog = new ContentDialog
        {
            XamlRoot = this.XamlRoot,
            Title = "Import Product Catalog",
            Content = content,
            PrimaryButtonText = "Import",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
        {
            return null;
        }

        var filePath = pathTextBox.Text?.Trim() ?? string.Empty;
        return string.IsNullOrWhiteSpace(filePath) ? null : filePath;
    }
}
