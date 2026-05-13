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

        // 1. Fetch Paid Orders
        var orders = await _orderRepository.GetAsync(o =>
            o.OrderDate >= start && o.OrderDate <= end && o.PaymentStatus == "Paid");

        // 2. Fetch Detailed Data
        var details = await _orderDetailRepository.GetAsync(
            od => od.Order!.OrderDate >= start && od.Order!.OrderDate <= end && od.Order!.PaymentStatus == "Paid",
            "Order", "Product.Category");

        // Apply Scope Filtering
        var filteredDetails = categoryScope == "All" 
            ? details 
            : details.Where(od => od.Product?.Category?.Name == categoryScope).ToList();

        var allLineMetrics = BuildLineMetrics(details);
        var filteredDetailIds = filteredDetails.Select(od => od.Id).ToHashSet();
        var lineMetrics = categoryScope == "All"
            ? allLineMetrics
            : allLineMetrics.Where(m => filteredDetailIds.Contains(m.DetailId)).ToList();

        // 3. Calculate Metrics
        data.Summary.OrdersCount = categoryScope == "All"
            ? orders.Count
            : filteredDetails.Select(od => od.OrderId).Distinct().Count();
        data.Summary.TotalRevenue = categoryScope == "All"
            ? orders.Sum(o => o.TotalAmount)
            : lineMetrics.Sum(m => m.Revenue);
        data.Summary.TotalProfit = lineMetrics.Sum(m => m.Profit);
        data.Summary.ProductsCount = filteredDetails.Sum(od => od.Quantity);

        // Fetch Expenses for net profit
        var expenses = await _expenseRepository.GetAsync(e => 
            e.Date >= start.ToUniversalTime() && e.Date <= end.ToUniversalTime());
        if (categoryScope == "All")
        {
            data.Summary.TotalProfit -= expenses.Sum(e => e.Amount);
        }

        // 4. Trends with Hole-Filling
        data.SalesTrend = GroupAndFillHoles(filteredDetails.Select(d => new TrendItem { Date = d.Order!.OrderDate, Value = d.Quantity }), start, end, granularity);
        var revenueTrendItems = categoryScope == "All"
            ? orders.Select(o => new TrendItem { Date = o.OrderDate, Value = (double)o.TotalAmount })
            : lineMetrics.Select(m => new TrendItem { Date = m.Date, Value = (double)m.Revenue });
        data.RevenueTrend = GroupAndFillHoles(revenueTrendItems, start, end, granularity);
        
        var grossProfitTrend = lineMetrics.Select(m => new TrendItem { Date = m.Date, Value = (double)m.Profit });
        var expenseTrend = categoryScope == "All" 
            ? expenses.Select(e => new TrendItem { Date = e.Date, Value = -(double)e.Amount }) 
            : Enumerable.Empty<TrendItem>();
            
        data.ProfitTrend = GroupAndFillHoles(grossProfitTrend.Concat(expenseTrend), start, end, granularity);

        // 5. Rankings (All products with 1+ sales)
        data.TopProducts = filteredDetails
            .GroupJoin(lineMetrics, od => od.Id, m => m.DetailId, (od, metrics) => new { Detail = od, Metric = metrics.FirstOrDefault() })
            .GroupBy(x => x.Detail.Product?.Name ?? "Unknown")
            .Select(g => new RankingPoint
            {
                Name = g.Key,
                Volume = g.Sum(x => x.Detail.Quantity),
                Amount = g.Sum(x => x.Metric?.Revenue ?? 0m)
            })
            .Where(x => x.Volume > 0)
            .OrderByDescending(x => x.Amount)
            .ToList();

        // 6. Category Leaders
        data.CategoryProfit = filteredDetails
            .GroupJoin(lineMetrics, od => od.Id, m => m.DetailId, (od, metrics) => new { Detail = od, Metric = metrics.FirstOrDefault() })
            .GroupBy(x => x.Detail.Product?.Category?.Name ?? "Others")
            .Select(g => new ProfitPoint
            {
                Category = g.Key,
                Amount = (double)g.Sum(x => x.Metric?.Revenue ?? 0m),
                Color = GetCategoryColor(g.Key)
            })
            .Where(x => x.Amount > 0)
            .OrderByDescending(x => x.Amount)
            .ToList();

        return data;
    }

    private class TrendItem { public DateTime Date; public double Value; }

    private class LineMetric
    {
        public int DetailId { get; set; }
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public decimal Profit { get; set; }
    }

    private static List<LineMetric> BuildLineMetrics(IEnumerable<OrderDetail> details)
    {
        return details
            .Where(d => d.Order != null)
            .GroupBy(d => d.OrderId)
            .SelectMany(group =>
            {
                var order = group.First().Order!;
                var grossSubtotal = group.Sum(d => d.Quantity * d.UnitPrice);
                var orderSubtotal = order.SubTotal > 0 ? order.SubTotal : grossSubtotal;
                var taxableBase = Math.Max(0, order.TotalAmount - order.TaxAmount);
                if (taxableBase == 0 && orderSubtotal > 0)
                {
                    taxableBase = Math.Max(0, orderSubtotal - order.DiscountAmount - order.ManualAdjustment);
                }

                return group.Select(detail =>
                {
                    var lineGross = detail.Quantity * detail.UnitPrice;
                    var ratio = grossSubtotal > 0 ? lineGross / grossSubtotal : 0;
                    var netSales = taxableBase * ratio;
                    var revenue = order.TotalAmount > 0 ? order.TotalAmount * ratio : netSales;
                    var cost = detail.Quantity * detail.UnitCostPrice;

                    return new LineMetric
                    {
                        DetailId = detail.Id,
                        Date = order.OrderDate,
                        Revenue = revenue,
                        Profit = netSales - cost
                    };
                });
            })
            .ToList();
    }

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
