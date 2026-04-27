using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;

namespace MyShop.Views.ProductManagement.Components;

public sealed partial class ProductFormView : ContentDialog
{
    public ProductFormViewModel? ViewModel => DataContext as ProductFormViewModel;

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

        if (ViewModel?.IsFormValid == true)
            this.Hide();
    }

    private async void ChooseImageFile_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".webp");

            if (Application.Current is App app && app.MainWindow != null)
            {
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(app.MainWindow);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            }

            var sourceFile = await picker.PickSingleFileAsync();
            if (sourceFile == null || ViewModel == null)
            {
                return;
            }

            var uploadRoot = Path.Combine(AppContext.BaseDirectory, "uploads", "products");
            Directory.CreateDirectory(uploadRoot);

            var extension = Path.GetExtension(sourceFile.Name);
            var targetFileName = $"product-{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension}";
            var targetPath = Path.Combine(uploadRoot, targetFileName);

            await CopyFileToPath(sourceFile, targetPath);

            ViewModel.ProductData.ImageUrl = targetPath;
            ViewModel.ValidationMessage = "Image file selected successfully.";
            ViewModel.ValidateProductFormCommand.Execute(null);
        }
        catch (Exception ex)
        {
            if (ViewModel != null)
            {
                ViewModel.ValidationMessage = $"Failed to select image: {ex.Message}";
            }
        }
    }

    private static async Task CopyFileToPath(StorageFile sourceFile, string destinationPath)
    {
        using var sourceStream = await sourceFile.OpenStreamForReadAsync();
        using var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await sourceStream.CopyToAsync(destinationStream);
    }
}