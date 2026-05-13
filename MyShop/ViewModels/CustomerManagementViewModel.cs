using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;
using MyShop.Services;
using MyShop.Data;

namespace MyShop.ViewModels;

public partial class CustomerManagementViewModel : ListManagementViewModelBase
{
    private readonly ICustomerService _customerService;
    private readonly INotificationService _notificationService;
    private readonly IAppConfigService _configService;

    [ObservableProperty]
    private int pageSize = 10;

    private readonly Action _onAdd;
    private readonly Action _onRules;
    private readonly Action<CustomerListItem> _onProfile;
    private readonly Action<CustomerListItem> _onEdit;

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
    [NotifyPropertyChangedFor(nameof(SortInfo))]
    private string sortColumn = "Name";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SortInfo))]
    private bool sortAscending = true;

    [ObservableProperty]
    private ObservableCollection<SortCriteriaItem> sortCriteriaList = new();

    public ObservableCollection<string> AvailableSortColumns { get; } = new()
    {
        "Name", "Phone", "Email", "Gender", "Birthday", "Tier", "Status", "TotalVisits", "TotalAccumulatedPoints"
    };

    public CustomerManagementViewModel(
        ICustomerService customerService,
        INotificationService notificationService,
        IAppConfigService configService,
        Action onAdd,
        Action onRules,
        Action<CustomerListItem> onProfile,
        Action<CustomerListItem> onEdit)
    {
        _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _onAdd = onAdd ?? throw new ArgumentNullException(nameof(onAdd));
        _onRules = onRules ?? throw new ArgumentNullException(nameof(onRules));
        _onProfile = onProfile ?? throw new ArgumentNullException(nameof(onProfile));
        _onEdit = onEdit ?? throw new ArgumentNullException(nameof(onEdit));
    }

    // --- PROPERTY CHANGE HANDLERS ---

    partial void OnCurrentPageChanged(int value)
    {
        RefreshCommand.ExecuteAsync(null);
    }

    partial void OnSearchTextChanged(string value)
    {
        CurrentPage = 1;
        RunDebounced(LoadCustomersAsync);
    }

    // --- MAIN LOAD LOGIC ---

    [RelayCommand]
    private async Task Refresh()
    {
        await LoadCustomersAsync();
    }

    private async Task LoadCustomersAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        try
        {
            // Fetch PageSize from config
            PageSize = await _configService.GetPageSizeAsync();

            string sortProperty = "";
            if (SortCriteriaList.Count > 0)
            {
                var sortStrings = new List<string>();
                foreach (var item in SortCriteriaList)
                {
                    sortStrings.Add($"{item.ColumnName} {item.Direction}");
                }
                sortProperty = string.Join(", ", sortStrings);
            }
            else
            {
                var direction = SortAscending ? "ASC" : "DESC";
                sortProperty = $"{SortColumn} {direction}";
            }

            var request = new PagingRequest
            {
                PageNumber = CurrentPage,
                PageSize = PageSize,
                SearchText = SearchText?.Trim(),
                SortBy = sortProperty
            };

            var result = await _customerService.GetAll(request);

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
            System.Diagnostics.Debug.WriteLine($"LoadCustomersAsync error: {ex.Message}");
            _notificationService.ShowError("Unable to load customers");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // --- HELPER METHODS ---

    private void UpdatePagingSummary(int currentItemsCount)
    {
        PagingSummary = BuildPagingSummary(
            CurrentPage,
            PageSize,
            TotalItems,
            currentItemsCount,
            "No customers found",
            "customers");
    }

    private void UpdatePageNumbers()
    {
        PageNumbers.Clear();
        for (int i = 1; i <= TotalPages; i++)
            PageNumbers.Add(i);

        PageInfoDisplay = BuildPageInfoDisplay(CurrentPage, TotalPages);
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

    // --- MULTI-CRITERIA COMMANDS ---

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
        await LoadCustomersAsync();
    }

    [RelayCommand]
    public async Task ClearMultiSort()
    {
        SortCriteriaList.Clear();
        CurrentPage = 1;
        await LoadCustomersAsync();
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
            _notificationService.ShowSuccess($"Deleted customer '{customer.Name}'");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($" Delete error: {ex.Message}");
            _notificationService.ShowError("Unable to delete customer");
        }
    }

    public string SortInfo => $"{SortColumn}|{SortAscending}";

    [RelayCommand]
    private async Task SortByColumn(string columnName)
    {
        if (string.IsNullOrWhiteSpace(columnName)) return;

        if (SortColumn == columnName)
        {
            SortAscending = !SortAscending;
        }
        else
        {
            SortColumn = columnName;
            SortAscending = true;
        }

        CurrentPage = 1;
        await LoadCustomersAsync();
    }

    // --- NAVIGATION COMMANDS ---

    [RelayCommand]
    private void AddCustomer() => _onAdd();

    [RelayCommand]
    private void NavigateToRules() => _onRules();

    [RelayCommand]
    private void NavigateToProfile(CustomerListItem customer)
    {
        if (customer != null)
            _onProfile(customer);
    }

    [RelayCommand]
    private void EditCustomer(CustomerListItem customer)
    {
        if (customer != null)
            _onEdit(customer);
    }

    public async Task UpdateCustomerStatusAsync(CustomerListItem customer, bool isActive)
    {
        if (customer == null)
            return;

        var oldStatus = customer.Status;
        var newStatus = isActive ? "Active" : "Inactive";

        try
        {
            await _customerService.UpdateStatus(customer.Id, newStatus);
            customer.Status = newStatus;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateCustomerStatusAsync error: {ex.Message}");
            _notificationService.ShowError("Unable to update customer status");
            customer.Status = oldStatus;
            await LoadCustomersAsync();
        }
    }
}

