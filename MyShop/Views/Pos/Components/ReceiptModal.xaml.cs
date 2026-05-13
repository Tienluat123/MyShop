using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;
using System.Linq;
using MyShop.Models;

namespace MyShop.Views.Pos.Components;

public sealed partial class ReceiptModal : ContentDialog
{
    private readonly PosOrder _order;
    private static bool _pdfLicenseConfigured;

    public ReceiptModal(PosOrder order)
    {
        this.InitializeComponent();
        _order = order;
        this.Loaded += ReceiptModal_Loaded;
    }

    private void ReceiptModal_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        this.DataContext = _order;
        
        OrderIdText.Text = $"Order #{_order.OrderId}";
        DateText.Text = _order.CreatedAt.ToString("g");
        ItemsList.ItemsSource = _order.Items;
        SubtotalText.Text = $"{_order.Subtotal:F2}VND";
        TaxText.Text = $"{_order.Tax:F2}VND";
        TotalText.Text = $"{_order.Total:F2}VND";

        CustomerNameText.Text = _order.CustomerName ?? "Guest";
        PaymentMethodText.Text = _order.Method.ToString();
        OrderTypeText.Text = _order.Type;
        
        if (!string.IsNullOrEmpty(_order.Table))
            OrderTypeText.Text += $" (Table {_order.Table})";
    }

    private async void PrintButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var outputPath = SaveReceiptPdf();

            var successDialog = new ContentDialog
            {
                XamlRoot = XamlRoot,
                Title = "Receipt exported",
                Content = $"PDF saved to:\n{outputPath}\n\nYou can print this file to PDF/XPS or share it directly.",
                CloseButtonText = "OK",
                RequestedTheme = ElementTheme.Default
            };

            await successDialog.ShowAsync();
        }
        catch (Exception ex)
        {
            var errorDialog = new ContentDialog
            {
                XamlRoot = XamlRoot,
                Title = "Unable to export receipt",
                Content = $"Could not create receipt PDF. {ex.Message}",
                CloseButtonText = "OK",
                RequestedTheme = ElementTheme.Default
            };

            await errorDialog.ShowAsync();
        }
    }

    private string SaveReceiptPdf()
    {
        if (!_pdfLicenseConfigured)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            _pdfLicenseConfigured = true;
        }

        var appRunFolder = AppContext.BaseDirectory;
        var folderPath = Path.Combine(appRunFolder, "Documents", "Receipts");
        Directory.CreateDirectory(folderPath);

        var safeOrderId = string.Concat((_order.OrderId ?? "unknown").Where(c => !Path.GetInvalidFileNameChars().Contains(c)));
        if (string.IsNullOrWhiteSpace(safeOrderId))
        {
            safeOrderId = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        }

        var filePath = Path.Combine(folderPath, $"receipt-{safeOrderId}-{DateTime.Now:yyyyMMdd-HHmmss}.pdf");
        var currency = "VND";

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5);
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().AlignCenter().Text("MYSHOP RECEIPT").SemiBold().FontSize(18);
                    col.Item().AlignCenter().Text($"Order #{_order.OrderId}");
                    col.Item().AlignCenter().Text(_order.CreatedAt.ToString("g"));
                });

                page.Content().PaddingVertical(12).Column(col =>
                {
                    col.Item().Text($"Customer: {_order.CustomerName ?? "Guest"}");
                    col.Item().Text($"Type: {_order.Type}{(string.IsNullOrWhiteSpace(_order.Table) ? string.Empty : $" (Table {_order.Table})")}");
                    col.Item().Text($"Payment: {_order.Method}");

                    col.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(5);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Item").SemiBold();
                            header.Cell().AlignRight().Text("Qty").SemiBold();
                            header.Cell().AlignRight().Text("Total").SemiBold();
                        });

                        foreach (var item in _order.Items)
                        {
                            table.Cell().Text(item.Product.Name);
                            table.Cell().AlignRight().Text(item.Quantity.ToString());
                            table.Cell().AlignRight().Text($"{item.TotalPrice:F2}{currency}");
                        }
                    });

                    col.Item().PaddingTop(12).AlignRight().Column(summary =>
                    {
                        summary.Item().Text($"Subtotal: {_order.Subtotal:F2}{currency}");
                        summary.Item().Text($"Tax: {_order.Tax:F2}{currency}");
                        summary.Item().Text($"Total: {_order.Total:F2}{currency}").SemiBold().FontSize(13);
                    });
                });

                page.Footer().AlignCenter().Text("Thank you for your visit!");
            });
        }).GeneratePdf(filePath);

        return filePath;
    }
}
