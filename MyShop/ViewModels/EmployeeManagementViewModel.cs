using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using MyShop.Models;
using MyShop.Services;
using MyShop.Data;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.ViewModels.StaffManagement;

/// <summary>
/// Quản lý nhân viên - Pure UI layer (no business logic)
/// Gọi Service để lấy dữ liệu đã tính toán và paginated từ DB
/// </summary>
public partial class EmployeeManagementViewModel : ListManagementViewModelBase
{
    private readonly IEmployeeService _employeeService;
    private readonly IScheduleService _scheduleService;
    private readonly INotificationService _notificationService;
    private readonly IAppConfigService _configService;
    private readonly Func<Task>? _onAddRequested;
    private readonly Func<UserSalaryInfo, Task>? _onEditRequested;

    [ObservableProperty]
    private int pageSize = 10;

    // --- DASHBOARD STATS ---
    [ObservableProperty]
    private string totalPayrollDisplay = "0 VND";

    [ObservableProperty]
    private int activeStaffCount = 0;

    [ObservableProperty]
    private string topPerformerName = "Updating...";

    // --- SEARCH & FILTER ---
    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string selectedRole = "All Roles";

    [ObservableProperty]
    private string selectedStatus = "All Status";

    // --- PAGING ---
    [ObservableProperty]
    private int currentPage = 1;

    [ObservableProperty]
    private int totalPages = 1;

    [ObservableProperty]
    private string pageInfoDisplay = "Page 1 / 1";

    // --- SORTING ---
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SortInfo))]
    private string sortColumn = "TotalHours";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SortInfo))]
    private bool sortAscending = false;

    [ObservableProperty]
    private ObservableCollection<SortCriteriaItem> sortCriteriaList = new();

    public ObservableCollection<string> AvailableSortColumns { get; } = new()
    {
        "FullName", "Phone", "Role", "Status", "HourlyRate", "TotalHours", "LastMonthSalary", "FinalSalary"
    };

    // --- LOADING STATE ---
    [ObservableProperty]
    private bool isLoading = false;

    // --- DISPLAY DATA ---
    [ObservableProperty]
    private ObservableCollection<UserSalaryInfo> displayEmployees = new();

    public EmployeeManagementViewModel(
        IEmployeeService employeeService,
        IScheduleService scheduleService,
        INotificationService notificationService,
        IAppConfigService configService,
        Func<Task>? onAddRequested = null,
        Func<UserSalaryInfo, Task>? onEditRequested = null)
    {
        _employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
        _scheduleService = scheduleService ?? throw new ArgumentNullException(nameof(scheduleService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _onAddRequested = onAddRequested;
        _onEditRequested = onEditRequested;

        _scheduleService.OnScheduleChanged += async () =>
        {
            await RefreshCommand.ExecuteAsync(null);
        };
    }

    // --- PROPERTY CHANGE HANDLERS ---

    partial void OnCurrentPageChanged(int value)
    {
        RefreshCommand.ExecuteAsync(null);
    }

    partial void OnSelectedRoleChanged(string value)
    {
        CurrentPage = 1;
        RunDebounced(LoadEmployeesAsync);
    }

    partial void OnSelectedStatusChanged(string value)
    {
        CurrentPage = 1;
        RunDebounced(LoadEmployeesAsync);
    }

    partial void OnSearchTextChanged(string value)
    {
        CurrentPage = 1;
        RunDebounced(LoadEmployeesAsync);
    }

    // --- MAIN REFRESH ---

    [RelayCommand]
    public async Task Refresh()
    {
        await LoadEmployeesAsync();
    }

    private async Task LoadEmployeesAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        try
        {
            // Load PageSize from config
            PageSize = await _configService.GetPageSizeAsync();

            // Build paging request with all filters
            string sortProperty = "";
            if (SortCriteriaList.Count > 0)
            {
                var sortStrings = new List<string>();
                foreach (var item in SortCriteriaList)
                {
                    var dir = item.Direction;
                    var mapped = item.ColumnName switch
                    {
                        "FullName" => $"User.FirstName {dir}, User.LastName {dir}",
                        "Phone" => $"User.Phone {dir}",
                        "Role" => $"User.Role {dir}",
                        "Status" => $"User.IsActive {dir}",
                        "HourlyRate" => $"User.HourlyRate {dir}",
                        "TotalHours" => $"CurrentHours {dir}",
                        "LastMonthSalary" => $"User.HourlyRate * LastMonthHours {dir}",
                        "FinalSalary" => $"User.HourlyRate * CurrentHours {dir}",
                        _ => $"CurrentHours {dir}"
                    };
                    sortStrings.Add(mapped);
                }
                sortProperty = string.Join(", ", sortStrings);
            }
            else
            {
                var direction = SortAscending ? "ASC" : "DESC";
                sortProperty = SortColumn switch
                {
                    "FullName" => $"User.FirstName {direction}, User.LastName {direction}",
                    "Phone" => $"User.Phone {direction}",
                    "Role" => $"User.Role {direction}",
                    "Status" => $"User.IsActive {direction}",
                    "HourlyRate" => $"User.HourlyRate {direction}",
                    "TotalHours" => $"CurrentHours {direction}",
                    "LastMonthSalary" => $"User.HourlyRate * LastMonthHours {direction}",
                    "FinalSalary" => $"User.HourlyRate * CurrentHours {direction}",
                    _ => "CurrentHours DESC"
                };
            }

            var request = new PagingRequest
            {
                PageNumber = CurrentPage,
                PageSize = PageSize,
                SearchText = SearchText?.Trim(),
                SortBy = sortProperty,
                Filters = new Dictionary<string, string>
                {
                    { "Role", SelectedRole == "All Roles" ? "" : SelectedRole },
                    { "Status", SelectedStatus == "All Status" ? "" : SelectedStatus }
                }
            };

            // Service returns paginated + calculated data from DB
            var pagedResult = await _employeeService.GetPagedEmployeeSalariesAsync(request);

            // Bind to UI
            DisplayEmployees = new ObservableCollection<UserSalaryInfo>(pagedResult.Items);
            TotalPages = pagedResult.Pagination.TotalPages;
            PageInfoDisplay = BuildPageInfoDisplay(CurrentPage, TotalPages);
            TotalPayrollDisplay = string.Format("{0:N0} VND", pagedResult.TotalPayroll);
            ActiveStaffCount = pagedResult.ActiveStaffCount;
            TopPerformerName = pagedResult.TopPerformerName;

        }
        catch (Exception)
        {
            _notificationService.ShowError("Failed to load employee data");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // --- ACTION COMMANDS ---

    [RelayCommand]
    public async Task UpdateUserStatusAsync(UserSalaryInfo info)
    {
        if (info?.User == null) return;

        try
        {
            await _employeeService.UpdateEmployeeAsync(info.User);
            if (SelectedStatus == "All Status")
            {
                await RefreshDashboardStatsAsync();
            }
            else
            {
                await LoadEmployeesAsync();
            }
        }
        catch (Exception)
        {
            _notificationService.ShowError("Failed to update status");
        }
    }

    private async Task RefreshDashboardStatsAsync()
    {
        var request = new PagingRequest
        {
            PageNumber = 1,
            PageSize = 1,
            SearchText = SearchText?.Trim(),
            Filters = new Dictionary<string, string>
            {
                { "Role", SelectedRole == "All Roles" ? "" : SelectedRole },
                { "Status", SelectedStatus == "All Status" ? "" : SelectedStatus }
            }
        };

        var result = await _employeeService.GetPagedEmployeeSalariesAsync(request);
        TotalPayrollDisplay = string.Format("{0:N0} VND", result.TotalPayroll);
        ActiveStaffCount = result.ActiveStaffCount;
        TopPerformerName = result.TopPerformerName;
    }

    [RelayCommand]
    public async Task AddEmployeeAsync()
    {
        if (_onAddRequested is not null)
        {
            await _onAddRequested();
        }
    }

    [RelayCommand]
    public async Task EditEmployeeAsync(UserSalaryInfo info)
    {
        if (info?.User == null) return;

        if (_onEditRequested is not null)
        {
            await _onEditRequested(info);
        }
    }

    [RelayCommand]
    public async Task DeleteEmployeeAsync(UserSalaryInfo info)
    {
        if (info?.User == null) return;

        try
        {
            await _employeeService.DeleteEmployeeAsync(info.User.Id);
            CurrentPage = 1;
            await RefreshCommand.ExecuteAsync(null);
            _notificationService.ShowSuccess("Employee deactivated successfully");
        }
        catch (Exception)
        {
            _notificationService.ShowError("Unable to deactivate employee");
        }
    }

    // --- PAGING COMMANDS ---

    [RelayCommand]
    public void PreviousPage()
    {
        if (CurrentPage > 1)
            CurrentPage--;
    }

    [RelayCommand]
    public void NextPage()
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
        await LoadEmployeesAsync();
    }

    [RelayCommand]
    public async Task ClearMultiSort()
    {
        SortCriteriaList.Clear();
        CurrentPage = 1;
        await LoadEmployeesAsync();
    }

    // --- XAML COMPATIBILITY ---

    public string SortInfo => $"{SortColumn}|{SortAscending}";

    [RelayCommand]
    public async Task SortByColumn(string columnName)
    {
        if (SortColumn == columnName)
        {
            SortAscending = !SortAscending;
        }
        else
        {
            SortColumn = columnName;
            SortAscending = false;
        }

        CurrentPage = 1;
        // Immediate sort - no debounce needed for sorting
        await LoadEmployeesAsync();
    }
}

/// <summary>
/// Model for employee salary information (used in paginated results)
/// </summary>
public class UserSalaryInfo
{
    public User User { get; set; } = null!;
    public double TotalHours { get; set; }
    public decimal FinalSalary { get; set; }
    public decimal LastMonthSalary { get; set; }
    public string BonusTag { get; set; } = "0%";
    public SolidColorBrush TagColor { get; set; } = null!;
}

public class EmployeeSalaryPageResult : PagedResult<UserSalaryInfo>
{
    public decimal TotalPayroll { get; set; }
    public int ActiveStaffCount { get; set; }
    public string TopPerformerName { get; set; } = "None";
}
