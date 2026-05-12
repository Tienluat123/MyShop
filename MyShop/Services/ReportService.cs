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
    private readonly IRepository<Expense> _expenseRepository;

    public ReportService(
        IRepository<Order> orderRepository,
        IRepository<Product> productRepository,
        IRepository<OrderDetail> orderDetailRepository,
        IRepository<Expense> expenseRepository)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _orderDetailRepository = orderDetailRepository ?? throw new ArgumentNullException(nameof(orderDetailRepository));
        _expenseRepository = expenseRepository ?? throw new ArgumentNullException(nameof(expenseRepository));
    }

    public async Task<ReportData> GetReportDataAsync(DateTime start, DateTime end, string granularity, string categoryScope = "All")
    {
        var data = new ReportData();

        // Ensure we only care about the Date part for grouping logic
        start = start.Date;
        end = end.Date.AddDays(1).AddTicks(-1);

        // 1. Fetch all detailed data for the period once
        // This includes all Paid orders and their details
        var details = await _orderDetailRepository.GetAsync(
            od => od.Order!.OrderDate >= start && od.Order!.OrderDate <= end && od.Order!.PaymentStatus == "Paid",
            "Order", "Product.Category");

        // 2. Pre-process details to calculate attributed Net Revenue and Net Profit
        // We use a ratio (TotalPaid / SubTotal) to attribute discounts and taxes proportionally to each item.
        var processed = details.Select(od =>
        {
            decimal orderSubtotal = od.Order?.SubTotal ?? 0;
            decimal orderTotalPaid = od.Order?.TotalAmount ?? 0;

            // Ratio accounts for both discounts and tax proportionally
            decimal ratio = orderSubtotal > 0 ? orderTotalPaid / orderSubtotal : 1;

            decimal itemGrossRevenue = od.Quantity * od.UnitPrice;
            decimal itemNetRevenue = itemGrossRevenue * ratio;

            // Profit = Net Revenue - Total cost for this item
            decimal itemNetProfit = itemNetRevenue - (od.Quantity * od.UnitCostPrice);

            return new
            {
                Detail = od,
                NetRevenue = itemNetRevenue,
                NetProfit = itemNetProfit,
                CategoryName = od.Product?.Category?.Name ?? "Others",
                Date = od.Order?.OrderDate ?? start
            };
        }).ToList();

        // 3. Filter by Category Scope
        var filtered = categoryScope == "All"
            ? processed
            : processed.Where(p => p.CategoryName == categoryScope).ToList();

        // 4. Populate Summary Stats
        data.Summary.OrdersCount = filtered.Select(p => p.Detail.OrderId).Distinct().Count();
        data.Summary.ProductsCount = filtered.Sum(p => p.Detail.Quantity);
        data.Summary.TotalRevenue = filtered.Sum(p => p.NetRevenue);
        data.Summary.TotalProfit = filtered.Sum(p => p.NetProfit);

        // Fetch Expenses for net profit (Global only)
        var expenses = await _expenseRepository.GetAsync(e =>
            e.Date >= start.ToUniversalTime() && e.Date <= end.ToUniversalTime());
        
        if (categoryScope == "All")
        {
            data.Summary.TotalProfit -= expenses.Sum(e => e.Amount);
        }

        // 5. Trends
        data.SalesTrend = GroupAndFillHoles(filtered.Select(p => new TrendItem { Date = p.Date, Value = p.Detail.Quantity }), start, end, granularity);
        data.RevenueTrend = GroupAndFillHoles(filtered.Select(p => new TrendItem { Date = p.Date, Value = (double)p.NetRevenue }), start, end, granularity);

        var netProfitTrendPerItem = filtered.Select(p => new TrendItem { Date = p.Date, Value = (double)p.NetProfit });
        var expenseTrend = categoryScope == "All"
            ? expenses.Select(e => new TrendItem { Date = e.Date, Value = -(double)e.Amount })
            : Enumerable.Empty<TrendItem>();

        data.ProfitTrend = GroupAndFillHoles(netProfitTrendPerItem.Concat(expenseTrend), start, end, granularity);

        // 6. Product Rankings
        data.TopProducts = filtered
            .GroupBy(p => p.Detail.Product?.Name ?? "Unknown")
            .Select(g => new RankingPoint
            {
                Name = g.Key,
                Volume = g.Sum(p => p.Detail.Quantity),
                Amount = g.Sum(p => p.NetProfit)
            })
            .Where(x => x.Volume > 0)
            .OrderByDescending(x => x.Amount)
            .ToList();

        // 7. Category Leaders (Comparison remains global to show context)
        data.CategoryProfit = processed
            .GroupBy(p => p.CategoryName)
            .Select(g => new ProfitPoint
            {
                Category = g.Key,
                Amount = (double)g.Sum(p => p.NetProfit),
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

    private DateTime GetStartOfWeek(DateTime dt) => dt.Date.AddDays(-(int)dt.DayOfWeek);
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
