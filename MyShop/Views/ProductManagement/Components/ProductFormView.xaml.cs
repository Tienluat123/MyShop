using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using MyShop.ViewModels;

namespace MyShop.Views.ProductManagement.Components;

public sealed partial class ProductFormView : ContentDialog
{
    public ProductFormViewModel? ViewModel => DataContext as ProductFormViewModel;

    public ProductFormView(ProductFormViewModel viewModel)
    {
        this.InitializeComponent();
        this.DataContext = viewModel;
        UpdateImagePreview(viewModel.ProductData.ImageUrl);
    }

    private void ImageUrlTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateImagePreview(ImageUrlTextBox.Text);
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
            UpdateImagePreview(targetPath);
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

    private void UpdateImagePreview(string? imagePathOrUrl)
    {
        if (string.IsNullOrWhiteSpace(imagePathOrUrl))
        {
            ImagePreview.Source = null;
            ImagePreview.Visibility = Visibility.Collapsed;
            ImagePreviewPlaceholder.Visibility = Visibility.Visible;
            return;
        }

        try
        {
            Uri previewUri;
            if (Uri.TryCreate(imagePathOrUrl.Trim(), UriKind.Absolute, out var absoluteUri))
            {
                previewUri = absoluteUri;
            }
            else
            {
                var fullPath = Path.GetFullPath(imagePathOrUrl.Trim());
                if (!File.Exists(fullPath))
                {
                    ImagePreview.Source = null;
                    ImagePreview.Visibility = Visibility.Collapsed;
                    ImagePreviewPlaceholder.Visibility = Visibility.Visible;
                    return;
                }

                previewUri = new Uri(fullPath);
            }

            var bitmap = new BitmapImage(previewUri);
            ImagePreview.Source = bitmap;
            ImagePreview.Visibility = Visibility.Visible;
            ImagePreviewPlaceholder.Visibility = Visibility.Collapsed;
        }
        catch
        {
            ImagePreview.Source = null;
            ImagePreview.Visibility = Visibility.Collapsed;
            ImagePreviewPlaceholder.Visibility = Visibility.Visible;
        }
    }
}