using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Models;
using MyShop.Services;
using MyShop.ViewModels;
using System;

namespace MyShop.Views.Pos.Components;

public sealed partial class PromoSelectionModal : ContentDialog
{
    public PromoViewModel ViewModel { get; }
    public bool WasCancelled { get; private set; }

    public PromoSelectionModal(decimal orderTotal, Customer? customer = null)
    {
        this.InitializeComponent();
        ViewModel = new PromoViewModel(App.Services.GetRequiredService<IPromotionService>());
        ViewModel.OrderTotal = orderTotal;
        ViewModel.Customer = customer;
        DataContext = ViewModel;
        _ = ViewModel.LoadActivePromotions();

        ViewModel.PromotionSelected += (promo) => this.Hide();
    }

    private void Promo_Click(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        if (button.DataContext is Models.Promotion promo)
        {
            ViewModel.SelectPromotionCommand.Execute(promo);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        WasCancelled = true;
        Hide();
    }
}
