using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
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
        UpdateImagePreview(viewModel.ProductData.ImageUrl, PrimaryImagePreview, PrimaryImagePreviewPlaceholder);
        UpdateImagePreview(viewModel.ProductData.SecondaryImageUrl1, SecondaryImage1Preview, SecondaryImage1PreviewPlaceholder);
        UpdateImagePreview(viewModel.ProductData.SecondaryImageUrl2, SecondaryImage2Preview, SecondaryImage2PreviewPlaceholder);
    }

    private void PrimaryImageUrlTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateImagePreview(PrimaryImageUrlTextBox.Text, PrimaryImagePreview, PrimaryImagePreviewPlaceholder);
    }

    private void SecondaryImage1UrlTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateImagePreview(SecondaryImage1UrlTextBox.Text, SecondaryImage1Preview, SecondaryImage1PreviewPlaceholder);
    }

    private void SecondaryImage2UrlTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateImagePreview(SecondaryImage2UrlTextBox.Text, SecondaryImage2Preview, SecondaryImage2PreviewPlaceholder);
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

    private async void ChoosePrimaryImageFile_Click(object sender, RoutedEventArgs e)
    {
        await PickImageFileAndAssignAsync(
            path => ViewModel!.ProductData.ImageUrl = path,
            () => ViewModel?.ProductData.ImageUrl,
            PrimaryImagePreview,
            PrimaryImagePreviewPlaceholder,
            "Primary image selected successfully.");
    }

    private async void ChooseSecondaryImage1File_Click(object sender, RoutedEventArgs e)
    {
        await PickImageFileAndAssignAsync(
            path => ViewModel!.ProductData.SecondaryImageUrl1 = path,
            () => ViewModel?.ProductData.SecondaryImageUrl1,
            SecondaryImage1Preview,
            SecondaryImage1PreviewPlaceholder,
            "Secondary image 1 selected successfully.");
    }

    private async void ChooseSecondaryImage2File_Click(object sender, RoutedEventArgs e)
    {
        await PickImageFileAndAssignAsync(
            path => ViewModel!.ProductData.SecondaryImageUrl2 = path,
            () => ViewModel?.ProductData.SecondaryImageUrl2,
            SecondaryImage2Preview,
            SecondaryImage2PreviewPlaceholder,
            "Secondary image 2 selected successfully.");
    }

    private async Task PickImageFileAndAssignAsync(
        Action<string> assignPath,
        Func<string?> getCurrentPath,
        Image previewImage,
        UIElement previewPlaceholder,
        string successMessage)
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

            assignPath(targetPath);
            ViewModel.ValidationMessage = successMessage;
            ViewModel.ValidateProductFormCommand.Execute(null);
            UpdateImagePreview(getCurrentPath(), previewImage, previewPlaceholder);
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

    private void UpdateImagePreview(string? imagePathOrUrl, Image previewImage, UIElement previewPlaceholder)
    {
        if (string.IsNullOrWhiteSpace(imagePathOrUrl))
        {
            previewImage.Source = null;
            previewImage.Visibility = Visibility.Collapsed;
            previewPlaceholder.Visibility = Visibility.Visible;
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
                    previewImage.Source = null;
                    previewImage.Visibility = Visibility.Collapsed;
                    previewPlaceholder.Visibility = Visibility.Visible;
                    return;
                }

                previewUri = new Uri(fullPath);
            }

            var bitmap = new BitmapImage(previewUri);
            previewImage.Source = bitmap;
            previewImage.Visibility = Visibility.Visible;
            previewPlaceholder.Visibility = Visibility.Collapsed;
        }
        catch
        {
            previewImage.Source = null;
            previewImage.Visibility = Visibility.Collapsed;
            previewPlaceholder.Visibility = Visibility.Visible;
        }
    }
}