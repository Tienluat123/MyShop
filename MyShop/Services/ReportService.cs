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

        // Ensure we only care about the Date part for grouping logic
        start = start.Date;
        end = end.Date.AddDays(1).AddTicks(-1);

        // 1. Fetch Summary Stats
        data.Summary.OrdersCount = await _orderRepository.CountAsync(o => 
            o.OrderDate >= start && o.OrderDate <= end && o.PaymentStatus == "Paid");

        // 2. Fetch Detailed Data
        var details = await _orderDetailRepository.GetAsync(
            od => od.Order!.OrderDate >= start && od.Order!.OrderDate <= end && od.Order!.PaymentStatus == "Paid",
            "Order", "Product.Category");

        // Apply Scope Filtering
        var filteredDetails = categoryScope == "All" 
            ? details 
            : details.Where(od => od.Product?.Category?.Name == categoryScope).ToList();

        // 3. Calculate Metrics
        if (categoryScope == "All")
        {
            var orders = await _orderRepository.GetAsync(o => 
                o.OrderDate >= start && o.OrderDate <= end && o.PaymentStatus == "Paid");
            data.Summary.TotalRevenue = orders.Sum(o => o.TotalAmount);
        }
        else
        {
            data.Summary.TotalRevenue = filteredDetails.Sum(od => od.Quantity * od.UnitPrice);
        }

        data.Summary.TotalProfit = filteredDetails.Sum(od => od.Quantity * (od.UnitPrice - od.UnitCostPrice));
        data.Summary.ProductsCount = filteredDetails.Select(od => od.ProductId).Distinct().Count();

        // 4. Trends with Hole-Filling
        data.SalesTrend = GroupAndFillHoles(filteredDetails.Select(d => new TrendItem { Date = d.Order!.OrderDate, Value = d.Quantity }), start, end, granularity);
        data.RevenueTrend = GroupAndFillHoles(filteredDetails.Select(d => new TrendItem { Date = d.Order!.OrderDate, Value = (double)(d.Quantity * d.UnitPrice) }), start, end, granularity);

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

        // 6. Category Leaders
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

    private List<TrendPoint> GroupAndFillHoles(IEnumerable<TrendItem> items, DateTime start, DateTime end, string granularity)
    {
        var grouped = (granularity switch
        {
            "Day" => items.GroupBy(x => x.Date.Date),
            "Week" => items.GroupBy(x => GetStartOfWeek(x.Date)),
            "Month" => items.GroupBy(x => new DateTime(x.Date.Year, x.Date.Month, 1)),
            "Year" => items.GroupBy(x => new DateTime(x.Date.Year, 1, 1)),
            _ => items.GroupBy(x => x.Date.Date)
        }).ToDictionary(g => g.Key, g => g.Sum(x => x.Value));

        var result = new List<TrendPoint>();
        var current = start.Date;

        if (granularity == "Day")
        {
            while (current <= end.Date)
            {
                var val = grouped.ContainsKey(current) ? (double)grouped[current] : 0;
                result.Add(new TrendPoint(current.ToString("dd/MM"), val));
                current = current.AddDays(1);
            }
        }
        else if (granularity == "Week")
        {
            current = GetStartOfWeek(current);
            while (current <= end.Date)
            {
                var val = grouped.ContainsKey(current) ? (double)grouped[current] : 0;
                result.Add(new TrendPoint("W" + GetWeekOfYear(current), val));
                current = current.AddDays(7);
            }
        }
        else if (granularity == "Month")
        {
            current = new DateTime(current.Year, current.Month, 1);
            while (current <= end.Date)
            {
                var val = grouped.ContainsKey(current) ? (double)grouped[current] : 0;
                result.Add(new TrendPoint(current.ToString("MMM"), val));
                current = current.AddMonths(1);
            }
        }
        else if (granularity == "Year")
        {
            int startYear = start.Year;
            int endYear = end.Year;

            for (int y = startYear; y <= endYear; y++)
            {
                var key = new DateTime(y, 1, 1);
                var val = grouped.ContainsKey(key) ? grouped[key] : 0;

                result.Add(new TrendPoint(y.ToString(), val));
            }
        }

        return result;
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
