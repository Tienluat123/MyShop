using System.Collections.Generic;

namespace MyShop.Models;

public class ReportSummary
{
    public int ProductsCount { get; set; }
    public int OrdersCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalProfit { get; set; }
}

public class TrendPoint
{
    public TrendPoint() { }
    public TrendPoint(string label, double value)
    {
        Label = label;
        Value = value;
    }
    public string Label { get; set; } = string.Empty;
    public double Value { get; set; }
}

public class ProfitPoint
{
    public ProfitPoint() { }
    public ProfitPoint(string category, double amount, string color)
    {
        Category = category;
        Amount = amount;
        Color = color;
    }
    public string Category { get; set; } = string.Empty;
    public double Amount { get; set; }
    public string Color { get; set; } = string.Empty;
}

public class RankingPoint
{
    public RankingPoint() { }
    public RankingPoint(string name, double volume, decimal amount)
    {
        Name = name;
        Volume = volume;
        Amount = amount;
    }
    public string Name { get; set; } = string.Empty;
    public double Volume { get; set; }
    public decimal Amount { get; set; }
}

public class ReportData
{
    public ReportSummary Summary { get; set; } = new();
    public List<TrendPoint> SalesTrend { get; set; } = new();
    public List<TrendPoint> RevenueTrend { get; set; } = new();
    public List<TrendPoint> ProfitTrend { get; set; } = new();
    public List<ProfitPoint> CategoryProfit { get; set; } = new();
    public List<RankingPoint> TopProducts { get; set; } = new();
}
