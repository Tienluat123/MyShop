using CommunityToolkit.Mvvm.ComponentModel;
using MyShop.Models;
using MyShop.Services;
using System.Collections.ObjectModel;

namespace MyShop.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IProductService _productService;
    private readonly IOrderService _orderService;

    [ObservableProperty] private int _productsCount;
    [ObservableProperty] private int _todayOrdersCount;
    [ObservableProperty] private decimal _todayRevenue;
    [ObservableProperty] private ObservableCollection<Product> _lowStockProducts = new();
    [ObservableProperty] private ObservableCollection<Order> _recentOrders = new();
    [ObservableProperty] private ObservableCollection<ChartDataPoint> _revenueTrend = new();
    [ObservableProperty] private ObservableCollection<TopProduct> _topProducts = new();

    public DashboardViewModel(IProductService productService, IOrderService orderService)
    {
        _productService = productService;
        _orderService = orderService;
        _ = LoadDashboardData();
    }

    public async Task LoadDashboardData()
{
    try 
    {
        // 1. Tổng số sản phẩm
        var products = await _productService.GetAll();
        ProductsCount = products.Pagination.TotalItems;

        // 2. Top 5 sản phẩm sắp hết hàng (số lượng < 5)
        var lowStock = await _productService.GetLowStockProducts(5, 5);
        LowStockProducts = new ObservableCollection<Product>(lowStock);

        // 3. Đơn hàng và doanh thu hôm nay (Chỉ tính đơn đã Paid)
        TodayOrdersCount = await _orderService.GetTodayOrderCount();
        TodayRevenue = await _orderService.GetTodayRevenue();

        // 4. Chi tiết 3 đơn hàng gần nhất
        var recent = await _orderService.GetRecentOrders(3);
        RecentOrders = new ObservableCollection<Order>(recent);

        // 5. Biểu đồ doanh thu tháng hiện tại
        var now = DateTime.Now;
        var trend = await _orderService.GetRevenueByDateInMonth(now.Month, now.Year);
        
        // Đảm bảo cập nhật UI Thread nếu cần
        RevenueTrend = new ObservableCollection<ChartDataPoint>(
            trend.Select(x => new ChartDataPoint { Label = x.Label, Value = (double)x.Value }));

        // 6. Top 5 bán chạy
        var topSelling = await _orderService.GetTopSellingProducts(5);
        TopProducts = new ObservableCollection<TopProduct>(
            topSelling.Select(x => new TopProduct { 
                Name = x.Name, 
                Amount = x.Quantity.ToString() 
            }));
    }
    catch (Exception ex)
    {
        // Log error hoặc thông báo qua NotificationService
    }
}

    public class ChartDataPoint
    {
        public string Label { get; set; } = string.Empty;
        public double Value { get; set; }
    }

    public class TopProduct
    {
        public string Name { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
    }
}