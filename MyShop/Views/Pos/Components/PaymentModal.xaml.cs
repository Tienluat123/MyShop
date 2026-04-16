using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;
using MyShop.Services;
using MyShop.Models;

namespace MyShop.Views.Pos.Components;

public sealed partial class PaymentModal : ContentDialog
{
    private readonly PaymentViewModel _viewModel;
    public event Action<PaymentMethod>? PaymentConfirmed;

    public PaymentModal(Order order, IOrderService orderService)
    {
        this.InitializeComponent();
        _viewModel = new PaymentViewModel(order, orderService);
        DataContext = _viewModel;
        
        _viewModel.PaymentConfirmed += (method) => 
        {
            PaymentConfirmed?.Invoke(method);
            this.Hide();
        };
    }

    private async void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.ReturnToDraftCommand.ExecuteAsync(null);
        this.Hide();
    }
}
