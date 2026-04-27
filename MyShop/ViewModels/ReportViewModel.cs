using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;
using MyShop.Services;

namespace MyShop.ViewModels;

public partial class ReportViewModel : ObservableObject
{
    private readonly IReportService _reportService;
    private readonly ICategoryService _categoryService;
    private System.Threading.CancellationTokenSource? _cts;

    public ReportViewModel(IReportService reportService, ICategoryService categoryService)
    {
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
        _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
        
        // Default filters
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

        CategoryScopes = new ObservableCollection<string> { "All" };
        SelectedCategoryScope = "All";
        SelectedTrendType = "Sales";
        _ = LoadCategoriesAsync();

        // Initialize Year/Month lists
        for (int i = 2020; i <= DateTime.Now.Year; i++) Years.Add(i);
        for (int i = 1; i <= 12; i++) Months.Add(i);

        StartYear = EndYear = DateTime.Now.Year;
        UpdateVisibility();

        // Data is refreshed via property changes during initialization
    }

    [ObservableProperty] private int productsCount;
    [ObservableProperty] private int ordersCount;
    [ObservableProperty] private decimal totalRevenue;
    [ObservableProperty] private decimal totalProfit;

    [ObservableProperty]
    private string selectedPeriod;

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
    [ObservableProperty] private string selectedTrendType;
    [ObservableProperty] private int currentPage = 1;
    [ObservableProperty] private int totalPages;
    private const int PageSize = 10;

    partial void OnStartDateChanged(DateTime value) => _ = RefreshData();
    partial void OnEndDateChanged(DateTime value) => _ = RefreshData();
    partial void OnSelectedCategoryScopeChanged(string value) => _ = RefreshData();
    partial void OnSelectedPeriodChanged(string value) 
    {
        UpdateVisibility();
        _ = RefreshData();
    }

    public ObservableCollection<TrendPoint> SalesTrend { get; } = new();
    public ObservableCollection<TrendPoint> RevenueTrend { get; } = new();
    public ObservableCollection<TrendPoint> ProfitTrend { get; } = new();
    public ObservableCollection<ProfitPoint> CategoryProfit { get; } = new();
    public ObservableCollection<RankingPoint> TopProducts { get; } = new();
    public ObservableCollection<RankingPoint> PagedTopProducts { get; } = new();

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

    partial void OnSelectedTrendTypeChanged(string value)
    {
        RequestChartRefresh?.Invoke(this, EventArgs.Empty);
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

    public async Task RefreshData()
    {
        _cts?.Cancel();
        _cts = new System.Threading.CancellationTokenSource();
        var token = _cts.Token;

        try
        {
            // Debounce slightly to allow multiple property changes to settle
            await Task.Delay(50, token);

            var data = await _reportService.GetReportDataAsync(StartDate, EndDate, SelectedPeriod, SelectedCategoryScope);
            if (token.IsCancellationRequested) return;

            // 1. Summary
            ProductsCount = data.Summary.ProductsCount;
            OrdersCount = data.Summary.OrdersCount;
            TotalRevenue = data.Summary.TotalRevenue;
            TotalProfit = data.Summary.TotalProfit;

            // 2. Trends
            SalesTrend.Clear();
            foreach (var p in data.SalesTrend) SalesTrend.Add(p);

            RevenueTrend.Clear();
            foreach (var p in data.RevenueTrend) RevenueTrend.Add(p);

            ProfitTrend.Clear();
            foreach (var p in data.ProfitTrend) ProfitTrend.Add(p);

            // 3. Rankings
            TopProducts.Clear();
            foreach (var p in data.TopProducts) TopProducts.Add(p);

            CategoryProfit.Clear();
            foreach (var p in data.CategoryProfit) CategoryProfit.Add(p);

            UpdatePagedRankings();
            RequestChartRefresh?.Invoke(this, EventArgs.Empty);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error refreshing report data: {ex.Message}");
        }
    }

    public async Task LoadCategoriesAsync()
    {
        try
        {
            var categories = await _categoryService.GetAll();
            foreach (var cat in categories)
            {
                if (!CategoryScopes.Contains(cat.Name))
                    CategoryScopes.Add(cat.Name);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading categories: {ex.Message}");
        }
    }

    public event EventHandler? RequestChartRefresh;

    private void UpdatePagedRankings()
    {
        TotalPages = (int)Math.Ceiling(TopProducts.Count / (double)PageSize);
        if (CurrentPage < 1) CurrentPage = 1;
        if (CurrentPage > TotalPages && TotalPages > 0) CurrentPage = TotalPages;

        PagedTopProducts.Clear();
        var items = TopProducts.Skip((CurrentPage - 1) * PageSize).Take(PageSize);
        foreach (var item in items) PagedTopProducts.Add(item);
    }

    [RelayCommand]
    private void NextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            UpdatePagedRankings();
        }
    }

    [RelayCommand]
    private void PrevPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            UpdatePagedRankings();
        }
    }
}
