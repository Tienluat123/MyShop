using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;
using MyShop.Models;

namespace MyShop.Views.Pos.Components;

public sealed partial class PaymentModal : ContentDialog
{
    private readonly PaymentViewModel _viewModel;
    public event Action<PaymentMethod>? PaymentConfirmed;

    public PaymentModal(double total, string orderId)
    {
        this.InitializeComponent();
        _viewModel = new PaymentViewModel(total, orderId);
        DataContext = _viewModel;
        
        _viewModel.PaymentConfirmed += (method) => 
        {
            PaymentConfirmed?.Invoke(method);
            this.Hide();
        };
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Hide();
    }
}
