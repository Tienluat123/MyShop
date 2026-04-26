using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyShop.Data;
using MyShop.Models;

namespace MyShop.Services;

public class ReportService : IReportService
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<OrderDetail> _orderDetailRepository;

    public ReportService(
        IRepository<Order> orderRepository,
        IRepository<Product> productRepository,
        IRepository<OrderDetail> orderDetailRepository)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _orderDetailRepository = orderDetailRepository ?? throw new ArgumentNullException(nameof(orderDetailRepository));
    }

    public async Task<ReportData> GetReportDataAsync(DateTime start, DateTime end, string granularity, string categoryScope = "All")
    {
        var data = new ReportData();

        // 1. Fetch Summary Stats using high-level methods
        data.Summary.ProductsCount = await _productRepository.CountAsync();
        data.Summary.OrdersCount = await _orderRepository.CountAsync(o => 
            o.OrderDate >= start && o.OrderDate <= end && o.PaymentStatus == "Paid");

        // 2. Fetch required data for complex calculations
        // We fetch OrderDetails with Product and Category info for scoping and profit
        var details = await _orderDetailRepository.GetAsync(
            od => od.Order!.OrderDate >= start && od.Order!.OrderDate <= end && od.Order!.PaymentStatus == "Paid",
            "Order", "Product.Category");

        // Apply Scope Filtering
        var filteredDetails = categoryScope == "All" 
            ? details 
            : details.Where(od => od.Product?.Category?.Name == categoryScope).ToList();

        // 3. Calculate Totals
        if (categoryScope == "All")
        {
            // For global scope, we can use the TotalAmount from Orders directly
            var orders = await _orderRepository.GetAsync(o => 
                o.OrderDate >= start && o.OrderDate <= end && o.PaymentStatus == "Paid");
            data.Summary.TotalRevenue = orders.Sum(o => o.TotalAmount);
        }
        else
        {
            data.Summary.TotalRevenue = filteredDetails.Sum(od => od.Quantity * od.UnitPrice);
        }

        data.Summary.TotalProfit = filteredDetails.Sum(od => od.Quantity * (od.UnitPrice - od.UnitCostPrice));

        // 4. Trends (Day/Week/Month/Year)
        data.SalesTrend = GroupByGranularity(filteredDetails.Select(d => new TrendItem { Date = d.Order!.OrderDate, Value = d.Quantity }), granularity);
        data.RevenueTrend = GroupByGranularity(filteredDetails.Select(d => new TrendItem { Date = d.Order!.OrderDate, Value = (double)(d.Quantity * d.UnitPrice) }), granularity);

        // 5. Rankings
        data.TopProducts = filteredDetails
            .GroupBy(od => od.Product?.Name ?? "Unknown")
            .Select(g => new RankingPoint
            {
                Name = g.Key,
                Volume = g.Sum(od => od.Quantity),
                Amount = g.Sum(od => (decimal)(od.Quantity * (od.UnitPrice - od.UnitCostPrice)))
            })
            .OrderByDescending(x => x.Amount)
            .Take(10)
            .ToList();

        // 6. Category Leaderboard (Always global or relative to scope?)
        // Usually, the pie chart shows category breakdown of the current scope or global.
        // Let's keep it global for better context.
        data.CategoryProfit = details
            .GroupBy(od => od.Product?.Category?.Name ?? "Others")
            .Select(g => new ProfitPoint
            {
                Category = g.Key,
                Amount = (double)g.Sum(od => od.Quantity * (od.UnitPrice - od.UnitCostPrice)),
                Color = GetCategoryColor(g.Key)
            })
            .OrderByDescending(x => x.Amount)
            .ToList();

        return data;
    }

    private class TrendItem { public DateTime Date; public double Value; }

    private List<TrendPoint> GroupByGranularity(IEnumerable<TrendItem> items, string granularity)
    {
        return granularity switch
        {
            "Day" => items.GroupBy(x => x.Date.Date)
                          .OrderBy(g => g.Key)
                          .Select(g => new TrendPoint(g.Key.ToString("dd/MM"), g.Sum(x => x.Value)))
                          .ToList(),
            "Week" => items.GroupBy(x => GetStartOfWeek(x.Date))
                           .OrderBy(g => g.Key)
                           .Select(g => new TrendPoint("W" + GetWeekOfYear(g.Key), g.Sum(x => x.Value)))
                           .ToList(),
            "Month" => items.GroupBy(x => new DateTime(x.Date.Year, x.Date.Month, 1))
                            .OrderBy(g => g.Key)
                            .Select(g => new TrendPoint(g.Key.ToString("MMM yy"), g.Sum(x => x.Value)))
                            .ToList(),
            "Year" => items.GroupBy(x => x.Date.Year)
                           .OrderBy(g => g.Key)
                           .Select(g => new TrendPoint(g.Key.ToString(), g.Sum(x => x.Value)))
                           .ToList(),
            _ => items.GroupBy(x => x.Date.Date)
                      .Select(g => new TrendPoint(g.Key.ToShortDateString(), g.Sum(x => x.Value)))
                      .ToList()
        };
    }

    private DateTime GetStartOfWeek(DateTime dt) => dt.AddDays(-(int)dt.DayOfWeek);
    private int GetWeekOfYear(DateTime dt) => System.Globalization.CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dt, System.Globalization.DateTimeFormatInfo.CurrentInfo.CalendarWeekRule, System.Globalization.DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek);

    private string GetCategoryColor(string? category) => category switch
    {
        "Coffee" => "#3B82F6",
        "Tea" => "#10B981",
        "Desserts" => "#F59E0B",
        "Food" => "#EF4444",
        _ => "#6B7280"
    };
}
