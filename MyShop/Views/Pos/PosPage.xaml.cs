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

        ViewModel.EditItemRequested += async (s, item) =>
        {
            var modal = new ProductCustomizationModal(item.Product, item);
            modal.XamlRoot = this.XamlRoot;

            var handler = new Action<OrderItem>(async editedItem =>
            {
                // Sync changes back to the draft order and database
                await ViewModel.UpdateCustomizedItem(item, editedItem);
            });

            modal.ViewModel.ItemConfirmed += handler;
            await modal.ShowAsync();
            modal.ViewModel.ItemConfirmed -= handler;
        };

        ViewModel.CheckoutRequested += async (s, e) =>
        {
            if (ViewModel.CurrentDraftOrder == null) return;

            // 1. Membership Selection (Loyalty)
            var memberModal = new MembershipSelectionModal();
            memberModal.XamlRoot = this.XamlRoot;

            Customer? selectedCustomer = null;
            memberModal.ViewModel.MemberSelected += (member) =>
            {
                selectedCustomer = member;
            };

            await memberModal.ShowAsync();

            // 2. Promotion Selection
            var promoModal = new PromoSelectionModal();
            promoModal.XamlRoot = this.XamlRoot;
            
            Models.Promotion? selectedPromo = null;
            promoModal.ViewModel.PromotionSelected += (promo) =>
            {
                selectedPromo = (Models.Promotion?)promo;
            };
            
            await promoModal.ShowAsync();
            await ViewModel.ApplyPromotion(selectedPromo);

            // 3. Payment Checkout
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

            if (isPaid)
            {
                var finalizedOrder = ViewModel.FinalizeOrder(selectedMethod, selectedCustomer);

                if (finalizedOrder != null)
                {
                    var receiptModal = new ReceiptModal(finalizedOrder);
                    receiptModal.XamlRoot = this.XamlRoot;
                    await receiptModal.ShowAsync();
                }
            }
            else
            {
                await ViewModel.RevertOrderToDraft();
            }
        };
    }

    // In PosPage.xaml.cs
    private async void ProductCard_Click(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        if (button.DataContext is not Product selectedProduct) return;

        var modal = new ProductCustomizationModal(selectedProduct);
        modal.XamlRoot = this.XamlRoot;

        // CHANGE THIS HANDLER:
        // Instead of ViewModel.Order.Add, call the ViewModel's database-aware method
        modal.ViewModel.ItemConfirmed += async (confirmedItem) =>
        {
            await ViewModel.AddCustomizedToCart(new AddCustomizedParams
            {
                Product = confirmedItem.Product,
                Quantity = confirmedItem.Quantity,
                Options = confirmedItem.DrinkOptions
            });
        };

        await modal.ShowAsync();
    }

    private async void ActiveOrders_Click(object sender, RoutedEventArgs e)
    {
        var modal = new OrderManagementModal();
        modal.XamlRoot = this.XamlRoot;
        await modal.ShowAsync();
    }
}
