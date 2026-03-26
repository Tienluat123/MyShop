using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MyShop.ViewModels;

public partial class CustomerManagementViewModel : ObservableObject
{
    private readonly List<CustomerListItem> allCustomers;
    private readonly Action onAdd;
    private readonly Action onRules;
    private readonly Action<CustomerListItem> onProfile;
    private readonly Action<CustomerListItem> onEdit;

    public ObservableCollection<CustomerListItem> DisplayCustomers { get; } = new();
    public ObservableCollection<int> PageNumbers { get; } = new();

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private CustomerListItem? selectedCustomer;

    [ObservableProperty]
    private int currentPage = 1;

    [ObservableProperty]
    private int totalItems;

    [ObservableProperty]
    private int totalPages = 1;

    [ObservableProperty]
    private string pagingSummary = "Showing 0-0 in 0 items";

    public CustomerManagementViewModel(
        Action onAdd,
        Action onRules,
        Action<CustomerListItem> onProfile,
        Action<CustomerListItem> onEdit)
    {
        this.onAdd = onAdd;
        this.onRules = onRules;
        this.onProfile = onProfile;
        this.onEdit = onEdit;

        allCustomers = BuildSeedData();
        RebuildList();
    }

    [RelayCommand]
    private void Search()
    {
        CurrentPage = 1;
        RebuildList();
    }

    [RelayCommand]
    private void GoToPage(int page)
    {
        if (page < 1 || page > TotalPages) return;
        CurrentPage = page;
        RebuildList();
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (CurrentPage <= 1) return;
        CurrentPage--;
        RebuildList();
    }

    [RelayCommand]
    private void NextPage()
    {
        if (CurrentPage >= TotalPages) return;
        CurrentPage++;
        RebuildList();
    }

    [RelayCommand]
    private void MapsToAdd() => onAdd();

    [RelayCommand]
    private void MapsToRules() => onRules();

    [RelayCommand]
    private void MapsToProfile()
    {
        if (SelectedCustomer is not null) onProfile(SelectedCustomer);
    }

    [RelayCommand]
    private void EditCustomer(CustomerListItem customer) => onEdit(customer);

    public void OpenProfile(CustomerListItem customer)
    {
        SelectedCustomer = customer;
        MapsToProfile();
    }

    partial void OnSearchTextChanged(string value)
    {
        Search();
    }

    private void RebuildList()
    {
        const int pageSize = 5;

        var query = SearchText?.Trim() ?? string.Empty;
        var filtered = string.IsNullOrWhiteSpace(query)
            ? allCustomers
            : allCustomers.Where(c =>
                c.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                c.Phone.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

        TotalItems = filtered.Count;
        TotalPages = Math.Max(1, (int)Math.Ceiling(filtered.Count / (double)pageSize));

        if (CurrentPage > TotalPages) CurrentPage = TotalPages;

        var pageItems = filtered.Skip((CurrentPage - 1) * pageSize).Take(pageSize).ToList();

        DisplayCustomers.Clear();
        foreach (var customer in pageItems)
        {
            DisplayCustomers.Add(customer);
        }

        PageNumbers.Clear();
        for (int i = 1; i <= TotalPages; i++) PageNumbers.Add(i);

        var start = filtered.Count == 0 ? 0 : ((CurrentPage - 1) * pageSize) + 1;
        var end = filtered.Count == 0 ? 0 : start + pageItems.Count - 1;
        PagingSummary = $"Showing {start}-{end} in {TotalItems} customers";
    }

    private static List<CustomerListItem> BuildSeedData()
    {
        return
        [
            new CustomerListItem { Id = 1, Code = "CUST-001", Name = "Nguyen Van A", Initials = "NVA", Phone = "0901234567", Tier = "Gold", Points = 1250, Visits = 25 },
            new CustomerListItem { Id = 2, Code = "CUST-002", Name = "Tran Thi B", Initials = "TTB", Phone = "0912345678", Tier = "Silver", Points = 450, Visits = 8 },
            new CustomerListItem { Id = 3, Code = "CUST-003", Name = "Le Van C", Initials = "LVC", Phone = "0987654321", Tier = "Regular", Points = 0, Visits = 1 },
            new CustomerListItem { Id = 4, Code = "CUST-004", Name = "Pham Van D", Initials = "PVD", Phone = "0933445566", Tier = "Bronze", Points = 150, Visits = 3 },
            new CustomerListItem { Id = 5, Code = "CUST-005", Name = "Hoang Thi E", Initials = "HTE", Phone = "0944556677", Tier = "Gold", Points = 2100, Visits = 40 },
            new CustomerListItem { Id = 6, Code = "CUST-006", Name = "Vo Van F", Initials = "VVF", Phone = "0955667788", Tier = "Silver", Points = 880, Visits = 12 },
            new CustomerListItem { Id = 7, Code = "CUST-007", Name = "Dang Thi G", Initials = "DTG", Phone = "0966778899", Tier = "Regular", Points = 50, Visits = 2 }
        ];
    }
}
