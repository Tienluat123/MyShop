using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Services;
using MyShop.Models;
using MyShop.Data;

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

/// <summary>
/// Item dùng trong danh sách hiển thị (chuyển từ Promotion entity)
/// </summary>
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
    [NotifyPropertyChangedFor(nameof(StatusText))]
    [NotifyPropertyChangedFor(nameof(IsCurrentlyActive))]
    [NotifyPropertyChangedFor(nameof(IsUpcoming))]
    [NotifyPropertyChangedFor(nameof(IsExpired))]
    private bool isActive;

    public static PromotionItem FromEntity(Promotion p)
    {
        var type = p.PromoType == "Percent" ? PromoType.Percent : PromoType.FixedAmount;
        var target = p.TargetAudience?.ToLowerInvariant() switch
        {
            "vip" => PromoTarget.VIP,
            "newcustomer" => PromoTarget.NewCustomer,
            "loyalty" => PromoTarget.Loyalty,
            _ => PromoTarget.All
        };

        return new PromotionItem
        {
            Id = p.Id,
            Code = p.PromoCode,
            Type = type,
            Value = p.DiscountValue,
            MinimumOrderAmount = p.MinOrderAmount,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            Target = target,
            IsActive = p.IsActive
        };
    }

    public Promotion ToEntity()
    {
        var promoTypeStr = Type == PromoType.Percent ? "Percent" : "FixedAmount";
        var targetStr = Target.ToString();

        return new Promotion
        {
            Id = this.Id,
            PromoCode = this.Code,
            PromoType = promoTypeStr,
            DiscountValue = this.Value,
            MinOrderAmount = this.MinimumOrderAmount,
            StartDate = this.StartDate,
            EndDate = this.EndDate,
            TargetAudience = targetStr,
            IsActive = this.IsActive
        };
    }

    public string TypeDisplay => Type == PromoType.Percent ? "Percent" : "Fixed Amount";
    public string ValueDisplay => Type == PromoType.Percent ? $"{Value:F0}%" : $"{Value:N0}đ";
    public string MinimumOrderDisplay => $"{MinimumOrderAmount:N0}đ";
    public string PeriodDisplay => $"{StartDate:dd/MM/yyyy} - {EndDate:dd/MM/yyyy}";
    public string TargetDisplay => Target.ToString();

    public bool IsExpired => DateTime.Now > EndDate;
    public bool IsUpcoming => IsActive && DateTime.Now < StartDate;
    public bool IsCurrentlyActive => IsActive && DateTime.Now >= StartDate && DateTime.Now <= EndDate;

    public string StatusText
    {
        get
        {
            if (!IsActive) return "Inactive";
            if (IsExpired) return "Expired";
            if (IsUpcoming) return "Upcoming";
            return "Active";
        }
    }

    /// <summary>
    /// Called automatically by CommunityToolkit when IsActive changes via TwoWay binding.
    /// Fires after the value has been updated. Only meaningful when user toggled (not during load).
    /// </summary>
    partial void OnIsActiveChanged(bool value)
    {
        IsActiveChanged?.Invoke(this, value);
    }

    /// <summary>
    /// Event raised when IsActive is changed by user interaction (via toggle switch).
    /// </summary>
    public event Action<PromotionItem, bool>? IsActiveChanged;
}

/// <summary>
/// ViewModel cho trang quản lý khuyến mãi.
/// Dùng list thay vì ObservableCollection để tránh lỗi
/// "Changes in data source are not allowed during layout" từ ItemsRepeater.
/// </summary>
public partial class PromotionsViewModel : ObservableObject
{
    private readonly IPromotionService _promoService;
    private readonly IAppConfigService _configService;
    private readonly Action onAdd;
    private readonly Action<PromotionItem> onEdit;
    private List<PromotionItem> _allPromotions = new(); // No longer used for in-memory paging, but kept if needed for other logic

    /// <summary>
    /// Cờ để View biết rằng đang trong quá trình cập nhật dữ liệu,
    /// ToggleSwitch_Toggled không nên xử lý.
    /// </summary>
    public bool IsUpdatingData { get; set; }

    /// <summary>
    /// Cờ nội bộ: True khi đang load/rebuild data từ database.
    /// Dùng để phân biệt: thay đổi từ user click (cần save) vs thay đổi do refresh từ DB.
    /// </summary>
    private bool _isLoadingInternal = false;

    /// <summary>
    /// Danh sách hiển thị trên UI - gán lại toàn bộ thay vì Clear+Add
    /// để tránh lỗi layout của ItemsRepeater.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<PromotionItem> pageItems = new();

    [ObservableProperty]
    private ObservableCollection<int> pageNumbers = new();

    [ObservableProperty]
    private string searchQuery = string.Empty;

    [ObservableProperty]
    private string selectedStatus = "All";

    [ObservableProperty]
    private int currentPage = 1;

    [ObservableProperty]
    private int pageSize = 10;

    [ObservableProperty]
    private int totalItems;

    [ObservableProperty]
    private int totalPages;

    [ObservableProperty]
    private string pagingSummary = "Showing 0-0 of 0 items";

    [ObservableProperty]
    private string pageInfoDisplay = "Page 1 / 1";

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private string? errorMessage;

    // ===== Sorting properties (mirroring EmployeeManagementViewModel) =====
    [ObservableProperty]
    private string sortColumnName = "Code";

    [ObservableProperty]
    private bool sortAscending = true;

    [ObservableProperty]
    private string sortInfo = "Code|True";

    [ObservableProperty]
    private ObservableCollection<SortCriteriaItem> sortCriteriaList = new();

    public ObservableCollection<string> AvailableSortColumns { get; } = new()
    {
        "Code", "Type", "Value", "Status", "MinOrder", "StartDate", "Target"
    };

    partial void OnSortColumnNameChanged(string value) => UpdateSortInfo();
    partial void OnSortAscendingChanged(bool value) => UpdateSortInfo();

    private void UpdateSortInfo()
    {
        SortInfo = $"{SortColumnName}|{SortAscending}";
    }

    public PromotionsViewModel(
        IPromotionService promoService,
        IAppConfigService configService,
        Action onAdd,
        Action<PromotionItem> onEdit)
    {
        _promoService = promoService ?? throw new ArgumentNullException(nameof(promoService));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        this.onAdd = onAdd ?? throw new ArgumentNullException(nameof(onAdd));
        this.onEdit = onEdit ?? throw new ArgumentNullException(nameof(onEdit));

        _ = RefreshFromDatabaseAsync();
    }

    [RelayCommand]
    private async Task LoadPromotionsAsync()
    {
        await RefreshFromDatabaseAsync();
    }

    public async Task RefreshFromDatabaseAsync()
    {
        try
        {
            _isLoadingInternal = true;
            IsUpdatingData = true;
            IsLoading = true;
            ErrorMessage = null;

            // Lấy PageSize từ cấu hình nếu chưa có hoặc load lại
            PageSize = await _configService.GetPageSizeAsync();

            string sortProperty = "";
            if (SortCriteriaList.Count > 0)
            {
                var sortStrings = new List<string>();
                foreach (var item in SortCriteriaList)
                {
                    var dir = item.Direction;
                    var mapped = item.ColumnName switch
                    {
                        "Code" => $"PromoCode {dir}",
                        "Type" => $"PromoType {dir}",
                        "Value" => $"DiscountValue {dir}",
                        "Status" => $"IsActive {dir}",
                        "MinOrder" => $"MinOrderAmount {dir}",
                        "StartDate" => $"StartDate {dir}",
                        "Target" => $"TargetAudience {dir}",
                        _ => $"PromoCode {dir}"
                    };
                    sortStrings.Add(mapped);
                }
                sortProperty = string.Join(", ", sortStrings);
            }
            else
            {
                var direction = SortAscending ? "ASC" : "DESC";
                sortProperty = SortColumnName switch
                {
                    "Code" => $"PromoCode {direction}",
                    "Type" => $"PromoType {direction}",
                    "Value" => $"DiscountValue {direction}",
                    "Status" => $"IsActive {direction}",
                    "MinOrder" => $"MinOrderAmount {direction}",
                    "StartDate" => $"StartDate {direction}",
                    "Target" => $"TargetAudience {direction}",
                    _ => $"PromoCode {direction}"
                };
            }

            var request = new PagingRequest
            {
                PageNumber = CurrentPage,
                PageSize = PageSize,
                SearchText = SearchQuery?.Trim(),
                SortBy = sortProperty,
                Filters = new Dictionary<string, string>
                {
                    { "Status", SelectedStatus }
                }
            };

            var pagedResult = await _promoService.GetAllAsync(request);
            
            TotalItems = pagedResult.Pagination.TotalItems;
            TotalPages = pagedResult.Pagination.TotalPages;

            PageItems.Clear();
            foreach (var p in pagedResult.Items)
            {
                var item = PromotionItem.FromEntity(p);
                item.IsActiveChanged += OnPromotionItemIsActiveChanged;
                PageItems.Add(item);
            }

            PageInfoDisplay = $"Page {CurrentPage} / {TotalPages}";
            UpdatePageNumbers();
            
            var start = TotalItems == 0 ? 0 : ((CurrentPage - 1) * PageSize) + 1;
            var end = TotalItems == 0 ? 0 : Math.Min(start + PageItems.Count - 1, TotalItems);
            PagingSummary = $"Showing {start}-{end} of {TotalItems} items";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi khi tải khuyến mãi: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            IsUpdatingData = false;
            _isLoadingInternal = false;
        }
    }

    private void UpdatePageNumbers()
    {
        var pageNums = new ObservableCollection<int>();
        for (int i = 1; i <= TotalPages; i++)
            pageNums.Add(i);
        PageNumbers = pageNums;
    }

    [RelayCommand]
    private async Task FilterByStatus(string status)
    {
        SelectedStatus = status;
        CurrentPage = 1;
        await RefreshFromDatabaseAsync();
    }

    [RelayCommand]
    private void AddPromotion() => onAdd();

    [RelayCommand]
    private void EditPromotion(PromotionItem promo) => onEdit(promo);

    /// <summary>
    /// Toggle trạng thái active/inactive.
    /// Dùng GetById để lấy entity đang track, flip IsActive, rồi Update.
    /// </summary>
    [RelayCommand]
    private async Task ToggleStatusAsync(PromotionItem promo)
    {
        if (promo == null) return;

        try
        {
            var entity = await _promoService.GetByIdAsync(promo.Id);
            if (entity == null)
            {
                ErrorMessage = $"Không tìm thấy khuyến mãi ID {promo.Id}";
                return;
            }

            entity.IsActive = !entity.IsActive;
            await _promoService.UpdateAsync(entity);

            await RefreshFromDatabaseAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi khi cập nhật trạng thái: {ex.Message}";
        }
    }

    /// <summary>
    /// Xử lý khi user toggle switch trên một PromotionItem.
    /// Chỉ được gọi khi user thực sự click toggle (không phải khi đang load data).
    /// </summary>
    private async void OnPromotionItemIsActiveChanged(PromotionItem promo, bool newValue)
    {
        // Bỏ qua nếu đang load data từ database
        if (_isLoadingInternal) return;

        try
        {
            // Lấy entity hiện tại từ database
            var entity = await _promoService.GetByIdAsync(promo.Id);
            if (entity == null) return;

            // Nếu giá trị trong DB đã = newValue → đồng bộ rồi, bỏ qua
            if (entity.IsActive == newValue) return;

            // Cập nhật vào database
            entity.IsActive = newValue;
            await _promoService.UpdateAsync(entity);

            // Chỉ refresh hiển thị từ server
            await RefreshFromDatabaseAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi khi cập nhật trạng thái: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeletePromotionAsync(PromotionItem promo)
    {
        if (promo == null) return;

        try
        {
            await _promoService.DeleteAsync(promo.Id);
            await RefreshFromDatabaseAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi khi xóa khuyến mãi: {ex.Message}";
        }
    }


    // === MULTI-CRITERIA COMMANDS ===

    [RelayCommand]
    public void AddSortCriteria()
    {
        SortCriteriaList.Add(new SortCriteriaItem(AvailableSortColumns));
    }

    [RelayCommand]
    public void RemoveSortCriteria(SortCriteriaItem item)
    {
        if (item != null && SortCriteriaList.Contains(item))
        {
            SortCriteriaList.Remove(item);
        }
    }

    [RelayCommand]
    public async Task ApplyMultiSort()
    {
        CurrentPage = 1;
        await RefreshFromDatabaseAsync();
    }

    [RelayCommand]
    public async Task ClearMultiSort()
    {
        SortCriteriaList.Clear();
        CurrentPage = 1;
        await RefreshFromDatabaseAsync();
    }

    // === Paging Commands ===

    [RelayCommand]
    private async Task GoToPage(int page)
    {
        if (page < 1 || page > TotalPages) return;
        CurrentPage = page;
        await RefreshFromDatabaseAsync();
    }

    [RelayCommand]
    private async Task NextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            await RefreshFromDatabaseAsync();
        }
    }

    [RelayCommand]
    private async Task PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await RefreshFromDatabaseAsync();
        }
    }

    [RelayCommand]
    private async Task SortByColumn(string columnName)
    {
        if (string.IsNullOrEmpty(columnName)) return;

        if (SortColumnName == columnName)
        {
            SortAscending = !SortAscending;
        }
        else
        {
            SortColumnName = columnName;
            SortAscending = true;
        }

        CurrentPage = 1;
        await RefreshFromDatabaseAsync();
    }

    // Auto-rebuild khi search thay đổi
    partial void OnSearchQueryChanged(string value)
    {
        CurrentPage = 1;
        _ = RefreshFromDatabaseAsync();
    }

    partial void OnSelectedStatusChanged(string value)
    {
        CurrentPage = 1;
        _ = RefreshFromDatabaseAsync();
    }
}
