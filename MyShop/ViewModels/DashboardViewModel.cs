using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;
using MyShop.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;

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

    private DateTime _lastLoadTime = DateTime.MinValue;

    [RelayCommand]
    public async Task LoadDashboardData()
    {
        // Tránh load lại quá nhanh (trong vòng 30s) nếu đã có dữ liệu
        if (DateTime.Now - _lastLoadTime < TimeSpan.FromSeconds(30) && RecentOrders.Any())
            return;

        try 
        {
            // Chạy các Task song song để tận dụng tối đa connection pool và giảm thời gian chờ
            var productsCountTask = _productService.GetTotalCount();
            var lowStockTask = _productService.GetLowStockProducts(5, 5);
            var todayOrdersTask = _orderService.GetTodayOrderCount();
            var todayRevenueTask = _orderService.GetTodayRevenue();
            
            var today = DateTime.Now;
            var rawTrendTask = _orderService.GetRevenueByDateInMonth(today.Month, today.Year);
            var recentTask = _orderService.GetRecentOrders(3);
            var topSellingTask = _orderService.GetTopSellingProducts(5);

            await Task.WhenAll(productsCountTask, lowStockTask, todayOrdersTask, todayRevenueTask, rawTrendTask, recentTask, topSellingTask);

            // Cập nhật UI sau khi tất cả Task hoàn tất
            ProductsCount = productsCountTask.Result;
            LowStockProducts = new ObservableCollection<Product>(lowStockTask.Result);
            TodayOrdersCount = todayOrdersTask.Result;
            TodayRevenue = todayRevenueTask.Result;

            // Xử lý dữ liệu biểu đồ
            var rawTrend = rawTrendTask.Result;
            var trend = new List<ChartDataPoint>();
            int daysToDisplay = today.Day;
            for (int i = 1; i <= daysToDisplay; i++)
            {
                string label = i.ToString();
                var dayData = rawTrend.FirstOrDefault(x => x.Label == label);
                trend.Add(new ChartDataPoint 
                { 
                    Label = label, 
                    Value = string.IsNullOrEmpty(dayData.Label) ? 0 : (double)dayData.Value 
                });
            }
            RevenueTrend = new ObservableCollection<ChartDataPoint>(trend);

            // Cập nhật danh sách đơn hàng và top sản phẩm
            RecentOrders = new ObservableCollection<Order>(recentTask.Result.OrderByDescending(o => o.OrderDate));
            TopProducts = new ObservableCollection<TopProduct>(
                topSellingTask.Result.Select(x => new TopProduct { 
                    Name = x.Name, 
                    Amount = x.Quantity.ToString() 
                }));

            _lastLoadTime = DateTime.Now;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DashboardVM] Load Error: {ex.Message}");
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