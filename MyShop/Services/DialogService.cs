using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace MyShop.Services;

public interface IDialogService
{
    Task<ContentDialogResult> ShowAsync(ContentDialog dialog, XamlRoot? xamlRoot = null);

    Task<ContentDialogResult> ShowMessageAsync(string title, string message, string primaryButtonText = "OK", string? secondaryButtonText = null);
}

public class DialogService : IDialogService
{
    private XamlRoot? GetCurrentXamlRoot()
        => (Application.Current as App)?.MainWindow?.Content?.XamlRoot;

    public async Task<ContentDialogResult> ShowAsync(ContentDialog dialog, XamlRoot? xamlRoot = null)
    {
        if (xamlRoot != null)
            dialog.XamlRoot = xamlRoot;
        else if (dialog.XamlRoot == null)
        {
            var currentXamlRoot = GetCurrentXamlRoot();
            if (currentXamlRoot != null)
                dialog.XamlRoot = currentXamlRoot;
        }

        try
        {
            return await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Dialog Error: {ex.Message}");
            return ContentDialogResult.None;
        }
    }

    public async Task<ContentDialogResult> ShowMessageAsync(string title, string message, string primaryButtonText = "OK", string? secondaryButtonText = null)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            PrimaryButtonText = primaryButtonText,
            SecondaryButtonText = secondaryButtonText,
            DefaultButton = ContentDialogButton.Primary
        };

        dialog.XamlRoot ??= GetCurrentXamlRoot();

        return await ShowAsync(dialog);
    }
}