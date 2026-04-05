using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Models;
using MyShop.Services;
using MyShop.ViewModels;
using MyShop.Views.Pos.Components;

namespace MyShop.Views.Pos;

public sealed partial class PosPage : Page
{
    public PosViewModel ViewModel { get; }

    public PosPage()
    {
        ViewModel = App.Services.GetRequiredService<PosViewModel>();
        this.DataContext = ViewModel;

        InitializeComponent();

        ViewModel.CheckoutRequested += async (s, e) =>
        {
            if (ViewModel.CurrentDraftOrder == null) return;

            var paymentModal = new PaymentModal(ViewModel.CurrentDraftOrder, App.Services.GetRequiredService<IOrderService>());
            paymentModal.XamlRoot = this.XamlRoot;

            bool isPaid = false;
            PaymentMethod selectedMethod = PaymentMethod.Cash;

            paymentModal.PaymentConfirmed += (method) =>
            {
                isPaid = true;
                selectedMethod = method;
            };

            await paymentModal.ShowAsync();

            if (!isPaid)
            {
                await ViewModel.RevertOrderToDraft();
            }
            else
            {
                // After payment, prompt for membership/loyalty
                var memberModal = new MembershipSelectionModal();
                memberModal.XamlRoot = this.XamlRoot;

                Customer? selectedCustomer = null;
                memberModal.ViewModel.MemberSelected += (member) =>
                {
                    selectedCustomer = member;
                };

                await memberModal.ShowAsync();

                // Finalize the order with payment method and member info
                var finalizedOrder = ViewModel.FinalizeOrder(selectedMethod, selectedCustomer);

                if (finalizedOrder != null)
                {
                    var receiptModal = new ReceiptModal(finalizedOrder);
                    receiptModal.XamlRoot = this.XamlRoot;
                    await receiptModal.ShowAsync();
                }
            }
        };
    }

    private async void ProductCard_Click(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        if (button.DataContext is not Product selectedProduct) return;

        if (!selectedProduct.HasCustomization)
        {
            ViewModel.AddToCart(selectedProduct);
            return;
        }

        var modal = new ProductCustomizationModal(selectedProduct);
        modal.XamlRoot = this.XamlRoot;

        modal.ViewModel.DrinkConfirmed += (opts, qty) =>
        {
            ViewModel.AddCustomizedToCart(new AddCustomizedParams
            {
                Product = selectedProduct,
                Options = opts,
                Quantity = qty
            });
        };

        await modal.ShowAsync();
    }

    private async void ActiveOrders_Click(object sender, RoutedEventArgs e)
    {
        var modal = new OrderManagementModal(ViewModel.ActiveOrders);
        modal.XamlRoot = this.XamlRoot;
        await modal.ShowAsync();
    }
}
