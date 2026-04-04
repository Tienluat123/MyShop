using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System.Collections.ObjectModel;
using MyShop.ViewModels;
using MyShop.Models;

namespace MyShop.Views.Pos.Components;

public sealed partial class OrderManagementModal : ContentDialog
{
    public OrderManagementViewModel ViewModel;

    public OrderManagementModal()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<OrderManagementViewModel>();
        DataContext = ViewModel;

        ViewModel.ScrollToTopAction = () =>
        {
            var scrollViewer = GetScrollViewer(OrderGridView);
            scrollViewer?.ChangeView(null, 0, null);
        };
    }

    private void CloseBtn_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        this.Hide();
    }

    private async void ContentDialog_Loaded(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            await ViewModel.LoadOrders();
        }
    }

    private void OrderGridView_Loaded(object sender, RoutedEventArgs e)
    {
        var scrollViewer = GetScrollViewer(OrderGridView);
        if (scrollViewer != null)
        {
            scrollViewer.ViewChanged += ScrollViewer_ViewChanged;
        }
    }

    private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
    {
        var sv = (ScrollViewer)sender;
        
        if (ViewModel.Filter != "Active") return;

        // Load more when scrolled near the end (e.g. 90% of the way down)
        if (sv.VerticalOffset >= sv.ScrollableHeight * 0.9)
        {
            if (ViewModel.LoadMoreOrdersCommand.CanExecute(null))
            {
                ViewModel.LoadMoreOrdersCommand.Execute(null);
            }
        }
    }

    private ScrollViewer GetScrollViewer(DependencyObject element)
    {
        if (element is ScrollViewer sv) return sv;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
        {
            var child = VisualTreeHelper.GetChild(element, i);
            var result = GetScrollViewer(child);
            if (result != null) return result;
        }

        return null;
    }
}
