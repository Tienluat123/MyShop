using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MyShop.ViewModels;

public partial class EmployeeListViewModel : ObservableObject
{
    private const string PositionAdmin = "Admin";
    private const string PositionSales = "Bán hàng";

    private const string StatusActive = "Đang làm";
    private const string StatusInactive = "Không hoạt động";

    private readonly List<EmployeeListItem> allEmployees;
    private readonly Action onAdd;
    private readonly Action<EmployeeListItem> onEdit;

    public ObservableCollection<EmployeeListItem> PageItems { get; } = new();
    public ObservableCollection<int> PageNumbers { get; } = new();

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string selectedPosition = "All";

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

    public EmployeeListViewModel(Action onAdd, Action<EmployeeListItem> onEdit)
    {
        this.onAdd = onAdd;
        this.onEdit = onEdit;

        allEmployees = BuildSeedEmployees();
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
    private void SelectPosition(string position)
    {
        if (string.IsNullOrWhiteSpace(position)) return;
        SelectedPosition = position;
        CurrentPage = 1;
        RebuildFilter();
    }

    [RelayCommand]
    private void AddEmployee() => onAdd();

    [RelayCommand]
    private void EditEmployee(EmployeeListItem employee) => onEdit(employee);

    partial void OnSearchTextChanged(string value) => RebuildFilter();

    public void RebuildFilter()
    {
        var query = SearchText?.Trim().ToLowerInvariant() ?? string.Empty;
        var filtered = allEmployees.Where(e =>
            (string.IsNullOrEmpty(query)
             || e.Name.ToLowerInvariant().Contains(query)
             || e.Code.ToLowerInvariant().Contains(query)
             || e.Phone.ToLowerInvariant().Contains(query))
            && (SelectedPosition == "All" || e.Position == SelectedPosition)).ToList();

        TotalItems = filtered.Count;
        TotalPages = (int)Math.Ceiling(TotalItems / (double)PageSize);
        if (TotalPages < 1) TotalPages = 1;
        if (CurrentPage > TotalPages) CurrentPage = TotalPages;
        if (CurrentPage < 1) CurrentPage = 1;

        RebuildPageItems(filtered);
    }

    private void RebuildPageItems(List<EmployeeListItem>? filtered = null)
    {
        var source = filtered ?? allEmployees.Where(e => (SelectedPosition == "All" || e.Position == SelectedPosition)).ToList();
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
        PagingSummary = $"Showing {start}-{end} in {TotalItems} staff members";
    }

    private static List<EmployeeListItem> BuildSeedEmployees()
    {
        return new List<EmployeeListItem>
        {
            new EmployeeListItem { Id = 1, Code = "NV001", Name = "Nguyễn Văn Nam", Initials = "VN", Phone = "0912.345.678", Position = PositionAdmin, Status = StatusActive, ShiftCount = 3 },
            new EmployeeListItem { Id = 2, Code = "NV002", Name = "Trần Thị Lan", Initials = "TL", Phone = "0987.654.321", Position = PositionSales, Status = StatusActive, ShiftCount = 0 },
            new EmployeeListItem { Id = 3, Code = "NV003", Name = "Lê Hoàng Quân", Initials = "HQ", Phone = "0905.111.222", Position = PositionSales, Status = StatusActive, ShiftCount = 2 },
            new EmployeeListItem { Id = 4, Code = "NV004", Name = "Phạm Gia Bảo", Initials = "GB", Phone = "0915.333.444", Position = PositionSales, Status = StatusInactive, ShiftCount = 0 },
            new EmployeeListItem { Id = 5, Code = "NV005", Name = "Đặng Minh Thu", Initials = "MT", Phone = "0944.555.666", Position = PositionSales, Status = StatusActive, ShiftCount = 5 },
            new EmployeeListItem { Id = 6, Code = "NV006", Name = "Vũ Anh Tuấn", Initials = "AT", Phone = "0933.777.888", Position = PositionSales, Status = StatusActive, ShiftCount = 1 }
        };
    }
}
