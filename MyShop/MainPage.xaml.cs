using MyShop.Views;
using MyShop.Views.AppConfig;
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;
using MyShop.Views.CustomerManagement;
using MyShop.Views.Pos;
using MyShop.Views.ProductManagement;
using MyShop.Views.Promotion;
using MyShop.Views.StaffManagement;

namespace MyShop.Views;

public sealed partial class MainPage : Page
{
    private MainPageViewModel? _viewModel;

    public MainPage()
    {
        this.InitializeComponent();

        // ✅ Set ViewModel as DataContext
        _viewModel = new MainPageViewModel();
        this.DataContext = _viewModel;

        // Set default page
        if (_viewModel.CurrentPageType != null)
        {
            ContentFrame.Navigate(_viewModel.CurrentPageType);
        }
    }

    /// <summary>
    /// Handle navigation khi user click menu item
    /// </summary>
    private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.InvokedItemContainer?.Tag is not string tag)
            return;

        // Map tag to page type
        Type? pageType = tag switch
        {
            "DashboardPage" => typeof(DashboardPage),
            "PosPage" => typeof(PosPage),
            "ManageProductPage" => typeof(ManageProductPage),
            "CustomersPage" => typeof(CustomerPage),
            "PromotionsPage" => typeof(PromotionsPage),
            "StaffSchedulePage" => typeof(StaffSchedulePage),
            "AppConfigPage" => typeof(AppConfigPage),
            _ => null
        };

        if (pageType != null)
        {
            // Update ViewModel current page
            if (_viewModel != null)
            {
                _viewModel.CurrentPageType = pageType;
            }

            // Navigate frame
            ContentFrame.Navigate(pageType);
        }
    }
}
