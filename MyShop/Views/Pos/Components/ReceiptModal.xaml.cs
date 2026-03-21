using Microsoft.UI.Xaml.Controls;
using MyShop.Models;

namespace MyShop.Views.Pos.Components;

public sealed partial class ReceiptModal : ContentDialog
{
    private PosOrder _order;

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
        SubtotalText.Text = $"${_order.Subtotal:F2}";
        TaxText.Text = $"${_order.Tax:F2}";
        TotalText.Text = $"${_order.Total:F2}";

        CustomerNameText.Text = _order.CustomerName ?? "Guest";
        PaymentMethodText.Text = _order.Method.ToString();
        OrderTypeText.Text = _order.Type;
        
        if (!string.IsNullOrEmpty(_order.Table))
            OrderTypeText.Text += $" (Table {_order.Table})";
    }
}
