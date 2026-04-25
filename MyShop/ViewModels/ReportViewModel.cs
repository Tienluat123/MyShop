using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MyShop.ViewModels;

public partial class ReportViewModel : ObservableObject
{
    public ReportViewModel()
    {
        // Default filters
        StartDate = DateTime.Now.AddDays(-30);
        EndDate = DateTime.Now;
        Periods = new ObservableCollection<string> { "Day", "Week", "Month", "Year" };
        SelectedPeriod = "Day";

        // Initialize Year/Month lists
        for (int i = 2020; i <= DateTime.Now.Year; i++) Years.Add(i);
        for (int i = 1; i <= 12; i++) Months.Add(i);

        StartYear = EndYear = DateTime.Now.Year;
        StartMonth = 1;
        EndMonth = DateTime.Now.Month;

        UpdateVisibility();

        // Mock Statistics
        ProductsCount = 1250;
        OrdersCount = 450;

        // Mock Sales Trend Data (Line Chart)
        SalesTrend = new ObservableCollection<TrendPoint>
        {
            new TrendPoint("Mon", 45),
            new TrendPoint("Tue", 52),
            new TrendPoint("Wed", 48),
            new TrendPoint("Thu", 70),
            new TrendPoint("Fri", 65),
            new TrendPoint("Sat", 85),
            new TrendPoint("Sun", 75)
        };

        // Mock Revenue Data (Bar Chart)
        RevenueData = new ObservableCollection<TrendPoint>
        {
            new TrendPoint("Week 1", 12000),
            new TrendPoint("Week 2", 15000),
            new TrendPoint("Week 3", 11000),
            new TrendPoint("Week 4", 18000)
        };

        // Mock Profit Data (Pie Chart)
        CategoryProfit = new ObservableCollection<ProfitPoint>
        {
            new ProfitPoint("Coffee", 4500000, "#3B82F6"),
            new ProfitPoint("Food", 2800000, "#10B981"),
            new ProfitPoint("Others", 1200000, "#F59E0B")
        };
    }

    [ObservableProperty]
    private int productsCount;

    [ObservableProperty]
    private int ordersCount;

    [ObservableProperty]
    private string selectedPeriod;

    partial void OnSelectedPeriodChanged(string value)
    {
        UpdateVisibility();
    }

    [ObservableProperty] private bool isDayWeekVisible;
    [ObservableProperty] private bool isMonthVisible;
    [ObservableProperty] private bool isYearVisible;

    [ObservableProperty] private int startMonth;
    [ObservableProperty] private int startYear;
    [ObservableProperty] private int endMonth;
    [ObservableProperty] private int endYear;

    public ObservableCollection<int> Years { get; } = new();
    public ObservableCollection<int> Months { get; } = new();

    private void UpdateVisibility()
    {
        IsDayWeekVisible = SelectedPeriod == "Day" || SelectedPeriod == "Week";
        IsMonthVisible = SelectedPeriod == "Month";
        IsYearVisible = SelectedPeriod == "Year";
    }

    [ObservableProperty]
    private DateTime startDate;

    [ObservableProperty]
    private DateTime endDate;

    public ObservableCollection<string> Periods { get; }

    public ObservableCollection<TrendPoint> SalesTrend { get; }
    public ObservableCollection<TrendPoint> RevenueData { get; }
    public ObservableCollection<ProfitPoint> CategoryProfit { get; }

    public class TrendPoint
    {
        public TrendPoint(string label, double value)
        {
            Label = label;
            Value = value;
        }
        public string Label { get; }
        public double Value { get; }
    }

    public class ProfitPoint
    {
        public ProfitPoint(string category, double amount, string color)
        {
            Category = category;
            Amount = amount;
            Color = color;
        }
        public string Category { get; }
        public double Amount { get; }
        public string Color { get; }
    }
}
