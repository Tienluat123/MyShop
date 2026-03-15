using MyShop.Views;
using System;
using Microsoft.UI.Xaml.Controls;

namespace MyShop.Views;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();

        // Set trang mặc định lúc mới mở app là Dashboard
        NavView.SelectedItem = NavView.MenuItems[0];
        ContentFrame.Navigate(typeof(DashboardPage));
    }

    private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.InvokedItemContainer != null && args.InvokedItemContainer.Tag != null)
        {
            string navItemTag = args.InvokedItemContainer.Tag.ToString();
            Type pageType = null;

            switch (navItemTag)
            {
                case "DashboardPage":
                    pageType = typeof(DashboardPage);
                    break;
                case "PosPage":
                    pageType = typeof(PosPage);
                    break;
                case "OrdersPage":
                    pageType = typeof(OrdersPage);
                    break;
                case "ManageProductPage":
                    pageType = typeof(ManageProductPage);
                    break;
                case "CustomersPage":
                    pageType = typeof(CustomersPage);
                    break;
                case "PromotionsPage":
                    pageType = typeof(PromotionsPage);
                    break;
                case "StaffSchedulePage":
                    pageType = typeof(StaffSchedulePage);
                    break;
                case "AppConfigPage":
                    pageType = typeof(AppConfigPage);
                    break;
                default:
                    pageType = typeof(DashboardPage);
                    break;
            }

            if (pageType != null && ContentFrame.CurrentSourcePageType != pageType)
            {
                ContentFrame.Navigate(pageType);
            }
        }
    }
}