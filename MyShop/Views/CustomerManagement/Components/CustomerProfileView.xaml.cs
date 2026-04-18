using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using MyShop.ViewModels;
using System;

namespace MyShop.Views.CustomerManagement.Components;

public sealed partial class CustomerProfileView : Page
{
    public CustomerProfileView()
    {
        this.InitializeComponent();
        this.Loaded += CustomerProfileView_Loaded;
    }

    private CustomerProfileViewModel? ViewModel => DataContext as CustomerProfileViewModel;

    private void CustomerProfileView_Loaded(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            _ = this.DispatcherQueue.TryEnqueue(async () => 
            {
                // 1. Đợi thật lâu (500ms) để trang cũ hoàn toàn biến mất
                await System.Threading.Tasks.Task.Delay(500); 
                
                // 2. Tự tay tạo TabView để tránh lỗi Geometry của XAML
                CreateSafeTabView();

                // 3. Nạp dữ liệu
                if (ViewModel.LoadInitialDataCommand.CanExecute(null))
                {
                    await ViewModel.LoadInitialDataCommand.ExecuteAsync(null);
                }
            });
        }
    }

    private void CreateSafeTabView()
{
    var tabView = new TabView
    {
        IsAddTabButtonVisible = false,
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch // Đảm bảo giãn hết chiều ngang
    };

    // 1. Tạo Tab Lịch sử đơn hàng
    var orderTab = new TabViewItem { Header = "Order History", IsClosable = false };
    
    var orderList = new ListView { 
        Margin = new Thickness(0, 10, 0, 0),
        // LẤY CÁI KHUÔN TỪ XAML RA ĐÂY 👇
        ItemTemplate = (DataTemplate)this.Resources["OrderRowTemplate"] 
    };

    // Nối dây dữ liệu
    orderList.SetBinding(ItemsControl.ItemsSourceProperty, new Binding { 
        Path = new PropertyPath("SelectedCustomer.Orders") 
    });

    orderTab.Content = orderList;
    tabView.TabItems.Add(orderTab);

    // 2. Tạo Tab Điểm thưởng (Tương tự)
    var pointsTab = new TabViewItem { Header = "Points", IsClosable = false };
    pointsTab.Content = new TextBlock { Text = "Loyalty points details coming soon...", Margin = new Thickness(20) };
    tabView.TabItems.Add(pointsTab);

    // Đưa toàn bộ vào cái hốc đã chờ sẵn
    DynamicTabContainer.Content = tabView;
}

    protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        this.DataContext = e.Parameter as CustomerProfileViewModel;
    }
}