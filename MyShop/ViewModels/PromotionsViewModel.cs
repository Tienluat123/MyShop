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

// 1. CHỈNH SỬA: Biến class này thành partial và kế thừa ObservableObject
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

    // Các thuộc tính hiển thị sẽ tự động cập nhật nhờ [NotifyPropertyChangedFor]
    public string TypeDisplay => Type == PromoType.Percent ? "Percent" : "Fixed Amount";
    public string ValueDisplay => Type == PromoType.Percent ? $"{Value:F0}%" : $"{Value:N0}đ";
    public string MinimumOrderDisplay => $"{MinimumOrderAmount:N0}đ";
    public string PeriodDisplay => $"{StartDate:dd/MM/yyyy} - {EndDate:dd/MM/yyyy}";
    public string TargetDisplay => Target.ToString();
}

public partial class PromotionsViewModel : ObservableObject
{
    public ObservableCollection<PromotionItem> Promotions { get; } = new();
    public ObservableCollection<PromotionItem> PageItems { get; } = new();

    [ObservableProperty]
    private string searchQuery = string.Empty;

    [ObservableProperty]
    private bool showOnlyActive = false;

    [ObservableProperty]
    private int currentPage = 1;

    [ObservableProperty]
    private int pageSize = 10;

    [ObservableProperty]
    private int totalItems;

    public string SummaryText => $"Showing {(PageItems.Count > 0 ? (CurrentPage - 1) * PageSize + 1 : 0)}-{Math.Min(CurrentPage * PageSize, TotalItems)} in {TotalItems} items";

    partial void OnTotalItemsChanged(int value) => OnPropertyChanged(nameof(SummaryText));

    [ObservableProperty]
    private int totalPages;

    public PromotionsViewModel()
    {
        LoadMockData();
        RebuildFilter();
    }

    partial void OnSearchQueryChanged(string value) => RebuildFilter();
    partial void OnShowOnlyActiveChanged(bool value) => RebuildFilter();

    [RelayCommand]
    private void SetPage(int page)
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
    private void AddPromotion()
    {
        var nextId = Promotions.Any() ? Promotions.Max(p => p.Id) + 1 : 1;
        var item = new PromotionItem
        {
            Id = nextId,
            Code = $"GIAM{nextId:D2}",
            Type = PromoType.Percent,
            Value = 30,
            MinimumOrderAmount = 100000,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(30),
            Target = PromoTarget.All,
            IsActive = true
        };

        Promotions.Insert(0, item);
        RebuildFilter();
    }

    [RelayCommand]
    private void ToggleActive(PromotionItem promo)
    {
        if (promo == null) return;
        promo.IsActive = !promo.IsActive;
        // Sau khi đổi trạng thái, có thể cần lọc lại nếu đang ở chế độ "ShowOnlyActive"
        if (ShowOnlyActive) RebuildFilter();
    }

    [RelayCommand]
    private void Remove(PromotionItem promo)
    {
        if (promo == null) return;
        Promotions.Remove(promo);
        RebuildFilter();
    }

    private void LoadMockData()
    {
        Promotions.Add(new PromotionItem
        {
            Id = 1,
            Code = "GIAM30",
            Type = PromoType.Percent,
            Value = 30,
            MinimumOrderAmount = 100000,
            StartDate = new DateTime(2026, 2, 1),
            EndDate = new DateTime(2026, 2, 28),
            Target = PromoTarget.VIP,
            IsActive = true
        });
        Promotions.Add(new PromotionItem
        {
            Id = 2,
            Code = "GIAM30",
            Type = PromoType.Percent,
            Value = 30,
            MinimumOrderAmount = 100000,
            StartDate = new DateTime(2026, 2, 1),
            EndDate = new DateTime(2026, 2, 28),
            Target = PromoTarget.All,
            IsActive = true
        });
        Promotions.Add(new PromotionItem
        {
            Id = 3,
            Code = "GIAM30",
            Type = PromoType.FixedAmount,
            Value = 30000,
            MinimumOrderAmount = 100000,
            StartDate = new DateTime(2026, 2, 1),
            EndDate = new DateTime(2026, 2, 28),
            Target = PromoTarget.All,
            IsActive = true
        });
    }

    public void RebuildFilter()
    {
        var query = SearchQuery?.Trim().ToLowerInvariant() ?? string.Empty;
        var filtered = Promotions.Where(p =>
            (string.IsNullOrEmpty(query)
             || p.Code.ToLowerInvariant().Contains(query)
             || p.TypeDisplay.ToLowerInvariant().Contains(query)
             || p.TargetDisplay.ToLowerInvariant().Contains(query))
            && (!ShowOnlyActive || p.IsActive));

        TotalItems = filtered.Count();
        TotalPages = (int)Math.Ceiling(TotalItems / (double)PageSize);
        if (TotalPages < 1) TotalPages = 1;
        if (CurrentPage > TotalPages) CurrentPage = TotalPages;
        if (CurrentPage < 1) CurrentPage = 1;

        RebuildPageItems(filtered);
    }

    private void RebuildPageItems(IEnumerable<PromotionItem>? filtered = null)
    {
        var source = filtered ?? Promotions.Where(p => (!ShowOnlyActive || p.IsActive));
        var taken = source
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        PageItems.Clear();
        foreach (var item in taken)
            PageItems.Add(item);
        
        // Cập nhật lại text thông báo số lượng
        OnPropertyChanged(nameof(SummaryText));
    }
}