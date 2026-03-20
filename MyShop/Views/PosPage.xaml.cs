using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Models;
using MyShop.ViewModels;
using MyShop.Views.Components;

namespace MyShop.Views;

public sealed partial class PosPage : Page
{
    public PosPage()
    {
        InitializeComponent();
        
        ViewModel.PlaceOrderRequested += async (s, e) =>
        {
            var orderId = $"ID-{DateTime.Now:Hmmss}";
            var paymentModal = new PaymentModal(ViewModel.Total, orderId);
            paymentModal.XamlRoot = this.XamlRoot;
            
            bool isPaid = false;
            PaymentMethod selectedMethod = PaymentMethod.Cash;

            paymentModal.PaymentConfirmed += (method) =>
            {
                isPaid = true;
                selectedMethod = method;
            };
            
            await paymentModal.ShowAsync();

            if (isPaid)
            {
                // After payment, prompt for membership/loyalty
                var memberModal = new MembershipSelectionModal();
                memberModal.XamlRoot = this.XamlRoot;
                
                TempCustomer? selectedCustomer = null;
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
        if (button.DataContext is not TempProduct selectedProduct) return;

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
