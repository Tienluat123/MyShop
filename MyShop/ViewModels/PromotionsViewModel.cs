using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MyShop.ViewModels;

public enum PromoType
{
    Percent,
    FixedAmount
}

public enum PromoTarget
{
    All,
    VIP,
    NewCustomer,
    Loyalty
}

public partial class PromotionItem : ObservableObject
{
    public int Id { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TypeDisplay))]
    [NotifyPropertyChangedFor(nameof(ValueDisplay))]
    private string code = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TypeDisplay))]
    [NotifyPropertyChangedFor(nameof(ValueDisplay))]
    private PromoType type;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ValueDisplay))]
    private decimal value;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MinimumOrderDisplay))]
    private decimal minimumOrderAmount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PeriodDisplay))]
    private DateTime startDate;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PeriodDisplay))]
    private DateTime endDate;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TargetDisplay))]
    private PromoTarget target;

    [ObservableProperty]
    private bool isActive;

    public string TypeDisplay => Type == PromoType.Percent ? "Percent" : "Fixed Amount";
    public string ValueDisplay => Type == PromoType.Percent ? $"{Value:F0}%" : $"{Value:N0}đ";
    public string MinimumOrderDisplay => $"{MinimumOrderAmount:N0}đ";
    public string PeriodDisplay => $"{StartDate:dd/MM/yyyy} - {EndDate:dd/MM/yyyy}";
    public string TargetDisplay => Target.ToString();
}

public partial class PromotionsViewModel : ObservableObject
{
    private readonly Action onAdd;
    private readonly Action<PromotionItem> onEdit;
    private readonly List<PromotionItem> allPromotions = new();

    public ObservableCollection<PromotionItem> PageItems { get; } = new();
    public ObservableCollection<int> PageNumbers { get; } = new();

    [ObservableProperty]
    private string searchQuery = string.Empty;

    [ObservableProperty]
    private bool showOnlyActive = false;

    [ObservableProperty]
    private int currentPage = 1;

    [ObservableProperty]
    private int pageSize = 5;

    [ObservableProperty]
    private int totalItems;

    [ObservableProperty]
    private int totalPages;

    [ObservableProperty]
    private string pagingSummary = "Showing 0-0 in 0 items";

    public PromotionsViewModel(Action onAdd, Action<PromotionItem> onEdit)
    {
        this.onAdd = onAdd;
        this.onEdit = onEdit;

        LoadMockData();
        RebuildFilter();
    }

    [RelayCommand]
    private void Search() => RebuildFilter();

    [RelayCommand]
    private void GoToPage(int page)
    {
        if (page < 1 || page > TotalPages) return;
        CurrentPage = page;
        RebuildPageItems();
    }

    [RelayCommand]
    private void NextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            RebuildPageItems();
        }
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            RebuildPageItems();
        }
    }

    [RelayCommand]
    private void AddPromotion() => onAdd();

    [RelayCommand]
    private void EditPromotion(PromotionItem promo) => onEdit(promo);

    [RelayCommand]
    private void ToggleStatus(PromotionItem promo)
    {
        if (promo == null) return;
        promo.IsActive = !promo.IsActive;
        if (ShowOnlyActive) RebuildFilter();
    }

    [RelayCommand]
    private void Remove(PromotionItem promo)
    {
        if (promo == null) return;
        allPromotions.Remove(promo);
        RebuildFilter();
    }

    private void LoadMockData()
    {
        allPromotions.Add(new PromotionItem { Id = 1, Code = "GIAM30", Type = PromoType.Percent, Value = 30, MinimumOrderAmount = 100000, StartDate = new DateTime(2026, 2, 1), EndDate = new DateTime(2026, 2, 28), Target = PromoTarget.VIP, IsActive = true });
        allPromotions.Add(new PromotionItem { Id = 2, Code = "SUMMER2026", Type = PromoType.Percent, Value = 15, MinimumOrderAmount = 50000, StartDate = new DateTime(2026, 6, 1), EndDate = new DateTime(2026, 8, 31), Target = PromoTarget.All, IsActive = true });
        allPromotions.Add(new PromotionItem { Id = 3, Code = "WELCOME50K", Type = PromoType.FixedAmount, Value = 50000, MinimumOrderAmount = 200000, StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 12, 31), Target = PromoTarget.NewCustomer, IsActive = true });
        allPromotions.Add(new PromotionItem { Id = 4, Code = "LOYALTY10", Type = PromoType.Percent, Value = 10, MinimumOrderAmount = 0, StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 12, 31), Target = PromoTarget.Loyalty, IsActive = false });
        allPromotions.Add(new PromotionItem { Id = 5, Code = "FLASH20", Type = PromoType.Percent, Value = 20, MinimumOrderAmount = 150000, StartDate = new DateTime(2026, 3, 20), EndDate = new DateTime(2026, 3, 25), Target = PromoTarget.All, IsActive = true });
    }

    public void AddNewPromotion(PromotionItem promo)
    {
        allPromotions.Insert(0, promo);
        RebuildFilter();
    }

    public void RebuildFilter()
    {
        var query = SearchQuery?.Trim().ToLowerInvariant() ?? string.Empty;
        var filtered = allPromotions.Where(p =>
            (string.IsNullOrEmpty(query)
             || p.Code.ToLowerInvariant().Contains(query)
             || p.TypeDisplay.ToLowerInvariant().Contains(query)
             || p.TargetDisplay.ToLowerInvariant().Contains(query))
            && (!ShowOnlyActive || p.IsActive)).ToList();

        TotalItems = filtered.Count;
        TotalPages = (int)Math.Ceiling(TotalItems / (double)PageSize);
        if (TotalPages < 1) TotalPages = 1;
        if (CurrentPage > TotalPages) CurrentPage = TotalPages;
        if (CurrentPage < 1) CurrentPage = 1;

        RebuildPageItems(filtered);
    }

    private void RebuildPageItems(List<PromotionItem>? filtered = null)
    {
        var source = filtered ?? allPromotions.Where(p => (!ShowOnlyActive || p.IsActive)).ToList();
        var taken = source
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        PageItems.Clear();
        foreach (var item in taken)
            PageItems.Add(item);

        PageNumbers.Clear();
        for (int i = 1; i <= TotalPages; i++)
            PageNumbers.Add(i);

        var start = source.Count == 0 ? 0 : ((CurrentPage - 1) * PageSize) + 1;
        var end = source.Count == 0 ? 0 : start + taken.Count - 1;
        PagingSummary = $"Showing {start}-{end} in {TotalItems} items";
    }

    partial void OnSearchQueryChanged(string value) => RebuildFilter();
    partial void OnShowOnlyActiveChanged(bool value) => RebuildFilter();
}