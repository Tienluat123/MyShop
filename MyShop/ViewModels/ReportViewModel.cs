using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MyShop.ViewModels;

public partial class ReportViewModel : ObservableObject
{
    public ReportViewModel()
    {
        // Default filters
        DatePresets = new ObservableCollection<string>
        {
            "Today", "Yesterday", "Last 7 days", "Last 30 days", "Last 3 months",
            "This month", "Last month", "This year", "Custom range..."
        };
        
        Periods = new ObservableCollection<string> { "Day", "Week", "Month", "Year" };
        AvailablePeriods = new ObservableCollection<string>();
        
        SelectedDatePreset = "Last 30 days";

        AnalyticsModes = new ObservableCollection<string> { "Top Products", "Top Categories" };
        SelectedAnalyticsMode = "Top Products";
        IsCategoryFilterEnabled = true;

        CategoryScopes = new ObservableCollection<string> { "All", "Coffee", "Tea", "Food", "Others" };
        SelectedCategoryScope = "All";

        // Initialize Year/Month lists
        for (int i = 2020; i <= DateTime.Now.Year; i++) Years.Add(i);
        for (int i = 1; i <= 12; i++) Months.Add(i);

        StartYear = EndYear = DateTime.Now.Year;
        StartMonth = 1;
        EndMonth = DateTime.Now.Month;

        UpdateVisibility();

        // Mock Statistics
        ProductsCount = 42;
        OrdersCount = 156;
        TotalRevenue = 12500.50m;
        TotalProfit = 4200.75m;

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

        TopProducts.Add(new RankingPoint("Latte Art Coffee", 156, 780.00m));
        TopProducts.Add(new RankingPoint("Green Tea Matcha", 124, 620.00m));
        TopProducts.Add(new RankingPoint("Chocolate Cake", 98, 490.00m));
        TopProducts.Add(new RankingPoint("Espresso Shot", 85, 255.00m));
        TopProducts.Add(new RankingPoint("Iced Americano", 76, 228.00m));
    }

    [ObservableProperty] private int productsCount;
    [ObservableProperty] private int ordersCount;
    [ObservableProperty] private decimal totalRevenue;
    [ObservableProperty] private decimal totalProfit;

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

    [ObservableProperty] private bool isCustomRangeVisible;
    [ObservableProperty] private string selectedDatePreset;
    [ObservableProperty] private string selectedAnalyticsMode;
    [ObservableProperty] private string selectedCategoryScope;
    [ObservableProperty] private bool isCategoryFilterEnabled;

    [ObservableProperty] private bool isTopProductsSelected = true;
    [ObservableProperty] private bool isTopCategoriesSelected;

    partial void OnIsTopProductsSelectedChanged(bool value)
    {
        if (value) SelectedAnalyticsMode = "Top Products";
    }

    partial void OnIsTopCategoriesSelectedChanged(bool value)
    {
        if (value) SelectedAnalyticsMode = "Top Categories";
    }

    partial void OnSelectedAnalyticsModeChanged(string value)
    {
        IsCategoryFilterEnabled = value == "Top Products";
        IsTopProductsSelected = value == "Top Products";
        IsTopCategoriesSelected = value == "Top Categories";
        if (!IsCategoryFilterEnabled) SelectedCategoryScope = "All";
    }

    public ObservableCollection<string> AnalyticsModes { get; }
    public ObservableCollection<string> CategoryScopes { get; }

    partial void OnSelectedDatePresetChanged(string value)
    {
        ApplyPreset();
    }

    private void ApplyPreset()
    {
        IsCustomRangeVisible = SelectedDatePreset == "Custom range...";
        var today = DateTime.Today;
        
        AvailablePeriods.Clear();

        switch (SelectedDatePreset)
        {
            case "Today":
            case "Yesterday":
                StartDate = SelectedDatePreset == "Today" ? today : today.AddDays(-1);
                EndDate = StartDate;
                AvailablePeriods.Add("Day");
                break;
            case "Last 7 days":
                StartDate = today.AddDays(-6);
                EndDate = today;
                AvailablePeriods.Add("Day");
                break;
            case "Last 30 days":
            case "This month":
            case "Last month":
                if (SelectedDatePreset == "Last 30 days")
                {
                    StartDate = today.AddDays(-29);
                    EndDate = today;
                }
                else if (SelectedDatePreset == "This month")
                {
                    StartDate = new DateTime(today.Year, today.Month, 1);
                    EndDate = StartDate.AddMonths(1).AddDays(-1);
                }
                else
                {
                    var lastMonth = today.AddMonths(-1);
                    StartDate = new DateTime(lastMonth.Year, lastMonth.Month, 1);
                    EndDate = StartDate.AddMonths(1).AddDays(-1);
                }
                AvailablePeriods.Add("Day");
                AvailablePeriods.Add("Week");
                break;
            case "Last 3 months":
                StartDate = today.AddMonths(-3).AddDays(1);
                EndDate = today;
                AvailablePeriods.Add("Day");
                AvailablePeriods.Add("Week");
                AvailablePeriods.Add("Month");
                break;
            case "This year":
                StartDate = new DateTime(today.Year, 1, 1);
                EndDate = new DateTime(today.Year, 12, 31);
                AvailablePeriods.Add("Week");
                AvailablePeriods.Add("Month");
                break;
            case "Custom range...":
                AvailablePeriods.Add("Day");
                AvailablePeriods.Add("Week");
                AvailablePeriods.Add("Month");
                AvailablePeriods.Add("Year");
                break;
        }

        if (!AvailablePeriods.Contains(SelectedPeriod))
        {
            SelectedPeriod = AvailablePeriods.FirstOrDefault() ?? "Day";
        }
    }

    public ObservableCollection<string> DatePresets { get; } = new();
    public ObservableCollection<string> AvailablePeriods { get; }

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
    public ObservableCollection<RankingPoint> TopProducts { get; } = new();

    public class RankingPoint
    {
        public RankingPoint(string name, double volume, decimal amount)
        {
            Name = name;
            Volume = volume;
            Amount = amount;
        }
        public string Name { get; }
        public double Volume { get; }
        public decimal Amount { get; }
    }

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
