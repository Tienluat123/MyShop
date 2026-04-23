using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using MyShop.Models;
using MyShop.Services;
using MyShop.Views.StaffManagement.Dialogs;
using MyShop.Data;
using MyShop.Di;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MyShop.ViewModels.StaffManagement;

/// <summary>
/// Quản lý nhân viên - Pure UI layer (no business logic)
/// Gọi Service để lấy dữ liệu đã tính toán và paginated từ DB
/// </summary>
public partial class EmployeeManagementViewModel : ObservableObject
{
    private readonly IEmployeeService _employeeService;
    private readonly IScheduleService _scheduleService;
    private readonly INotificationService _notificationService;
    private readonly IDialogService _dialogService;
    private CancellationTokenSource? _debounceToken;

    private const int PageSize = 10;

    // --- DASHBOARD STATS ---
    [ObservableProperty]
    private string totalPayrollDisplay = "0 đ";

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
    private int currentPageNumber = 1;

    [ObservableProperty]
    private int totalPages = 1;

    [ObservableProperty]
    private string pageInfoDisplay = "Page 1 / 1";

    // --- SORTING ---
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SortInfo))]
    private string sortColumnName = "TotalHours";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SortInfo))]
    private bool sortAscending = false;

    // --- LOADING STATE ---
    [ObservableProperty]
    private bool isLoading = false;

    // --- DISPLAY DATA ---
    [ObservableProperty]
    private ObservableCollection<UserSalaryInfo> displayEmployees = new();

    public EmployeeManagementViewModel(
        IEmployeeService employeeService,
        IDialogService dialogService,
        IScheduleService scheduleService,
        INotificationService notificationService)
    {
        _employeeService = employeeService;
        _dialogService = dialogService;
        _scheduleService = scheduleService;
        _notificationService = notificationService;

        _scheduleService.OnScheduleChanged += async () =>
        {
            await RefreshCommand.ExecuteAsync(null);
        };
    }

    // --- PROPERTY CHANGE HANDLERS ---

    partial void OnCurrentPageNumberChanged(int value)
    {
        RefreshCommand.ExecuteAsync(null);
    }

    partial void OnSelectedRoleChanged(string value)
    {
        CurrentPageNumber = 1;
        DebounceSearch();
    }

    partial void OnSelectedStatusChanged(string value)
    {
        CurrentPageNumber = 1;
        DebounceSearch();
    }

    partial void OnSearchTextChanged(string value)
    {
        CurrentPageNumber = 1;
        DebounceSearch();
    }

    // --- DEBOUNCE SEARCH (300ms) ---

    private async void DebounceSearch()
    {
        _debounceToken?.Cancel();
        _debounceToken = new CancellationTokenSource();

        try
        {
            await Task.Delay(300, _debounceToken.Token);
            await LoadEmployeesAsync();
        }
        catch (OperationCanceledException)
        {
            // User changed filter again - ignore
        }
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
            // Build paging request with all filters
            var request = new PagingRequest
            {
                PageNumber = CurrentPageNumber,
                PageSize = PageSize,
                SearchText = SearchText?.Trim(),
                SortColumn = SortColumnName,
                SortAscending = SortAscending,
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
            PageInfoDisplay = $"Page {CurrentPageNumber} / {TotalPages}";

            // Calculate dashboard stats from current page data
            CalculateDashboardStats(pagedResult.Items);

            System.Diagnostics.Debug.WriteLine($"✅ Loaded {pagedResult.Items.Count} employees (Page {CurrentPageNumber}/{TotalPages})");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ LoadEmployeesAsync error: {ex.Message}");
            _notificationService.ShowError("Lỗi khi tải dữ liệu nhân viên");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void CalculateDashboardStats(List<UserSalaryInfo> employees)
    {
        if (employees.Count == 0)
        {
            TotalPayrollDisplay = "0 đ";
            ActiveStaffCount = 0;
            TopPerformerName = "None";
            return;
        }

        // Total salary from current page (for dashboard display)
        decimal totalSalary = employees.Sum(x => x.FinalSalary);
        TotalPayrollDisplay = string.Format("{0:N0} đ", totalSalary);

        // Active count (from current page)
        ActiveStaffCount = employees.Count(x => x.User.IsActive);

        // Top performer by hours
        var topStaff = employees.OrderByDescending(x => x.TotalHours).FirstOrDefault();
        TopPerformerName = topStaff != null ? $"{topStaff.User.FullName} ({topStaff.TotalHours}h)" : "None";
    }

    // --- ACTION COMMANDS ---

    [RelayCommand]
    public async Task UpdateUserStatusAsync(UserSalaryInfo info)
    {
        if (info?.User == null) return;

        try
        {
            await _employeeService.UpdateEmployeeAsync(info.User);
            ActiveStaffCount = DisplayEmployees.Count(x => x.User.IsActive);
            System.Diagnostics.Debug.WriteLine($" Updated status for {info.User.FullName}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($" Update status error: {ex.Message}");
            _notificationService.ShowError("Lỗi khi cập nhật trạng thái");
        }
    }

    [RelayCommand]
    public async Task AddEmployeeAsync()
    {
        var formViewModel = new EmployeeFormViewModel();
        var dialog = new EmployeeFormDialog(formViewModel);
        dialog.SetEmployee(null);

        var result = await _dialogService.ShowAsync(dialog);

        if (dialog.SavedUser != null)
        {
            try
            {
                await _employeeService.AddEmployeeAsync(dialog.SavedUser);
                await RefreshCommand.ExecuteAsync(null);
                _notificationService.ShowSuccess($"Thêm nhân viên '{dialog.SavedUser.FullName}' thành công");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Add error: {ex.Message}");
                _notificationService.ShowError("Không thể thêm nhân viên");
            }
        }
    }

    [RelayCommand]
    public async Task EditEmployeeAsync(UserSalaryInfo info)
    {
        if (info?.User == null) return;

        var formViewModel = new EmployeeFormViewModel();
        var dialog = new EmployeeFormDialog(formViewModel);
        dialog.SetEmployee(info.User);

        var result = await _dialogService.ShowAsync(dialog);

        if (dialog.SavedUser != null)
        {
            try
            {
                await _employeeService.UpdateEmployeeAsync(dialog.SavedUser);
                await RefreshCommand.ExecuteAsync(null);
                _notificationService.ShowSuccess($"Cập nhật '{dialog.SavedUser.FullName}' thành công");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Edit error: {ex.Message}");
                _notificationService.ShowError("Không thể cập nhật nhân viên");
            }
        }
    }

    [RelayCommand]
    public async Task DeleteEmployeeAsync(UserSalaryInfo info)
    {
        if (info?.User == null) return;

        try
        {
            await _employeeService.DeleteEmployeeAsync(info.User.Id);
            CurrentPageNumber = 1;
            await RefreshCommand.ExecuteAsync(null);
            _notificationService.ShowSuccess("Xóa nhân viên thành công");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($" Delete error: {ex.Message}");
            _notificationService.ShowError("Không thể xóa nhân viên");
        }
    }

    // --- PAGING COMMANDS ---

    [RelayCommand]
    public void PreviousPage()
    {
        if (CurrentPageNumber > 1)
            CurrentPageNumber--;
    }

    [RelayCommand]
    public void NextPage()
    {
        if (CurrentPageNumber < TotalPages)
            CurrentPageNumber++;
    }

    // --- XAML COMPATIBILITY ---

    public string SortInfo => $"{SortColumnName}|{SortAscending}";

    [RelayCommand]
    public async Task SortByColumn(string columnName)
    {
        if (SortColumnName == columnName)
        {
            SortAscending = !SortAscending;
        }
        else
        {
            SortColumnName = columnName;
            SortAscending = false;
        }

        CurrentPageNumber = 1;
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

