using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;
using MyShop.Services;
using MyShop.Data;

namespace MyShop.ViewModels;

public partial class CustomerManagementViewModel : ObservableObject
{
    private readonly ICustomerService _customerService;
    private const int PageSize = 10;
    private CancellationTokenSource? _searchDebounceToken;

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

    [ObservableProperty]
    private string pageInfoDisplay = "Page 1 / 1";

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private string sortColumn = "Name";

    [ObservableProperty]
    private bool sortAscending = true;

    public CustomerManagementViewModel(
        ICustomerService customerService,
        Action onAdd,
        Action onRules,
        Action<CustomerListItem> onProfile,
        Action<CustomerListItem> onEdit)
    {
        _customerService = customerService;
        this.onAdd = onAdd;
        this.onRules = onRules;
        this.onProfile = onProfile;
        this.onEdit = onEdit;
    }

    // --- PROPERTY CHANGE HANDLERS ---

    partial void OnCurrentPageChanged(int value)
    {
        LoadCustomersCommand.ExecuteAsync(null);
    }

    partial void OnSearchTextChanged(string? value)
    {
        CurrentPage = 1;
        DebounceSearch();
    }

    // --- DEBOUNCE SEARCH ---

    private async void DebounceSearch()
    {
        // Hủy request tìm kiếm trước đó
        _searchDebounceToken?.Cancel();
        _searchDebounceToken = new CancellationTokenSource();

        try
        {
            // Chờ 300ms trước khi query
            await Task.Delay(300, _searchDebounceToken.Token);
            await LoadCustomersAsync();
        }
        catch (OperationCanceledException)
        {
            // Người dùng đã gõ tiếp → ignore request này
        }
    }

    // --- MAIN LOAD LOGIC ---

    [RelayCommand]
    private async Task LoadCustomers()
    {
        await LoadCustomersAsync();
    }

    private async Task LoadCustomersAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        try
        {
            var request = new PagingRequest
            {
                PageNumber = CurrentPage,
                PageSize = PageSize,
                SearchText = SearchText?.Trim(),
                SortColumn = SortColumn,
                SortAscending = SortAscending
            };

            var result = await _customerService.GetAll(request);
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Loaded {result.Items.Count} customers (Page {CurrentPage})");

            DisplayCustomers.Clear();
            foreach (var c in result.Items)
            {
                DisplayCustomers.Add(new CustomerListItem
                {
                    Id = c.Id,
                    Name = c.Name,
                    Phone = c.Phone,
                    Email = c.Email,
                    Gender = c.Gender,
                    Birthday = c.Birthday,
                    Code = $"CUST-{c.Id:D3}",
                    Tier = c.Tier.ToString(),
                    Status = c.Status,
                    TotalAccumulatedPoints = c.TotalAccumulatedPoints,
                    CurrentAvailablePoints = c.CurrentAvailablePoints,
                    Visits = c.TotalVisits,
                    Initials = string.Join("", c.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(x => x[0]))
                });
            }

            TotalItems = result.Pagination.TotalItems;
            TotalPages = result.Pagination.TotalPages;

            UpdatePagingSummary(result.Items.Count);
            UpdatePageNumbers();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ LoadCustomersAsync error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // --- HELPER METHODS ---

    private void UpdatePagingSummary(int currentItemsCount)
    {
        if (TotalItems == 0)
        {
            PagingSummary = "No customers found";
            return;
        }

        var start = ((CurrentPage - 1) * PageSize) + 1;
        var end = start + currentItemsCount - 1;
        PagingSummary = $"Showing {start}-{end} of {TotalItems} customers";
    }

    private void UpdatePageNumbers()
    {
        PageNumbers.Clear();
        for (int i = 1; i <= TotalPages; i++)
            PageNumbers.Add(i);

        PageInfoDisplay = $"Page {CurrentPage} / {TotalPages}";
    }

    // --- PAGING COMMANDS ---

    [RelayCommand]
    private void GoToPage(int page)
    {
        if (page < 1 || page > TotalPages) return;
        CurrentPage = page;
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (CurrentPage > 1)
            CurrentPage--;
    }

    [RelayCommand]
    private void NextPage()
    {
        if (CurrentPage < TotalPages)
            CurrentPage++;
    }

    // --- ACTION COMMANDS ---

    [RelayCommand]
    private async Task DeleteCustomer(CustomerListItem customer)
    {
        if (customer == null) return;

        try
        {
            await _customerService.Delete(customer.Id);
            CurrentPage = 1;
            await LoadCustomersAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Delete error: {ex.Message}");
        }
    }

    // --- NAVIGATION COMMANDS ---

    [RelayCommand]
    private void MapsToAdd() => onAdd();

    [RelayCommand]
    private void MapsToRules() => onRules();

    [RelayCommand]
    private void MapsToProfile(CustomerListItem customer)
    {
        if (customer != null)
            onProfile(customer);
    }

    [RelayCommand]
    private void EditCustomer(CustomerListItem customer)
    {
        if (customer != null)
            onEdit(customer);
    }
}

