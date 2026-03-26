using System;
using System.Collections.ObjectModel;
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

    public ObservableCollection<EmployeeListItem> Employees { get; } = new();

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string selectedPosition = "All";

    public EmployeeListViewModel()
    {
        allEmployees = BuildSeedEmployees();
        RebuildList();
    }

    [RelayCommand]
    private void Search()
    {
        RebuildList();
    }

    [RelayCommand]
    private void SelectPosition(string position)
    {
        if (string.IsNullOrWhiteSpace(position))
        {
            return;
        }

        SelectedPosition = position;
        RebuildList();
    }

    private void RebuildList()
    {
        var filtered = allEmployees.AsEnumerable();

        // Filter by search text
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLowerInvariant();
            filtered = filtered.Where(e =>
                e.Name.Contains(searchLower, StringComparison.OrdinalIgnoreCase) ||
                e.Code.Contains(searchLower, StringComparison.OrdinalIgnoreCase) ||
                e.Phone.Contains(searchLower, StringComparison.OrdinalIgnoreCase));
        }

        // Filter by position
        if (SelectedPosition != "All")
        {
            filtered = filtered.Where(e => e.Position == SelectedPosition);
        }

        Employees.Clear();
        foreach (var employee in filtered)
        {
            Employees.Add(employee);
        }
    }

    private static List<EmployeeListItem> BuildSeedEmployees()
    {
        return new List<EmployeeListItem>
        {
            new EmployeeListItem
            {
                Id = 1,
                Code = "NV001",
                Name = "Nguyễn Văn Nam",
                Initials = "VN",
                Phone = "0912.345.678",
                Position = PositionAdmin,
                Status = StatusActive,
                ShiftCount = 3,
                PositionColor = "{StaticResource ErrorStrong}",
                StatusColor = "{StaticResource SuccessStrong}"
            },
            new EmployeeListItem
            {
                Id = 2,
                Code = "NV002",
                Name = "Trần Thị Lan",
                Initials = "TL",
                Phone = "0987.654.321",
                Position = PositionSales,
                Status = StatusActive,
                ShiftCount = 0,
                PositionColor = "{StaticResource InfoStrong}",
                StatusColor = "{StaticResource SuccessStrong}"
            }
        };
    }
}
