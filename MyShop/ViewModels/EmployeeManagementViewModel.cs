using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using MyShop.Models;
using MyShop.Services;
using MyShop.Views.StaffManagement.Dialogs;
using MyShop.Di;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.ViewModels.StaffManagement;

public partial class EmployeeManagementViewModel : ObservableObject
{
    private readonly IEmployeeService _employeeService;
    private readonly IScheduleService _scheduleService;
    private readonly INotificationService _notificationService;
    private List<UserSalaryInfo> _allCalculatedData = new();

    [ObservableProperty] private string _totalPayrollDisplay = "0 đ";
    [ObservableProperty] private int _activeStaffCount = 0;
    [ObservableProperty] private string _topPerformerName = "Updating...";
    [ObservableProperty] private string _searchText = string.Empty;

    [ObservableProperty] private string _selectedRole = "All Roles";
    [ObservableProperty] private string _selectedStatus = "All Status";

    // Paging properties
    [ObservableProperty] private int _currentPageNumber = 1;
    [ObservableProperty] private int _pageSize = 10;
    [ObservableProperty] private int _totalPages = 1;
    [ObservableProperty] private string _pageInfoDisplay = "Page 1 / 1";

    // Sorting properties
    [ObservableProperty] private string _sortColumnName = "TotalHours";
    [ObservableProperty] private bool _sortAscending = false;
    [ObservableProperty] private string _sortInfo = "TotalHours|False";

    partial void OnSortColumnNameChanged(string value) => UpdateSortInfo();
    partial void OnSortAscendingChanged(bool value) => UpdateSortInfo();

    private void UpdateSortInfo()
    {
        SortInfo = $"{SortColumnName}|{SortAscending}";
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();
    partial void OnSelectedRoleChanged(string value) => ApplyFilter();
    partial void OnSelectedStatusChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        var term = SearchText?.ToLower().Trim() ?? "";

        var filtered = _allCalculatedData.Where(x =>
        {
            string searchableContent = string.Join(" ",
                x.User.FullName ?? "",
                x.User.Username ?? "",
                x.User.Phone ?? "",
                x.User.Role ?? "").ToLower();
            bool matchesSearch = string.IsNullOrEmpty(term) || searchableContent.Contains(term);

            bool matchesRole = SelectedRole == "All Roles" || string.IsNullOrEmpty(SelectedRole) ||
                (SelectedRole == "Admin" && x.User.Role == "1") ||
                (SelectedRole == "Sale" && x.User.Role == "2");

            bool matchesStatus = SelectedStatus == "All Status" ||
                                (SelectedStatus == "Active" ? x.User.IsActive : !x.User.IsActive);

            return matchesSearch && matchesRole && matchesStatus;
        }).ToList();

        var sorted = ApplySorting(filtered);

        CurrentPageNumber = 1;

        TotalPages = (int)Math.Ceiling((decimal)sorted.Count / PageSize);
        PageInfoDisplay = $"Page {CurrentPageNumber} / {TotalPages}";

        var pagedList = sorted.Skip(0).Take(PageSize).ToList();
        DisplayEmployees = new ObservableCollection<UserSalaryInfo>(pagedList);
    }

    // Danh sách chính Binding lên XAML
    [ObservableProperty] private ObservableCollection<UserSalaryInfo> _displayEmployees = new();

    private readonly IDialogService _dialogService;

    public EmployeeManagementViewModel(IEmployeeService employeeService, IDialogService dialogService, IScheduleService scheduleService, INotificationService notificationService)
    {
        _employeeService = employeeService;
        _dialogService = dialogService;
        _scheduleService = scheduleService;
        _notificationService = notificationService;

        _scheduleService.OnScheduleChanged += async () =>
        {
            await RefreshDashboardCommand.ExecuteAsync(null);
        };
    }


    [RelayCommand]
    public async Task RefreshDashboardAsync()
    {
        var now = DateTime.Now;
        var lastMonth = now.AddMonths(-1);

        var currentMonthList = await _employeeService.GetEmployeesWithSalaryStatsAsync(now.Month, now.Year);
        var lastMonthList = await _employeeService.GetEmployeesWithSalaryStatsAsync(lastMonth.Month, lastMonth.Year);

        decimal grandTotal = 0;
        var displayList = new List<UserSalaryInfo>();

        double maxHours = currentMonthList.Any() ? currentMonthList.Max(u =>
            u.WorkSchedules
                .Where(s => s.WorkDate.Month == now.Month && s.WorkDate.Year == now.Year && s.Status == "Completed")
                .Sum(s => s.TotalHours)) : 0;

        foreach (var user in currentMonthList)
        {
            double currentHours = user.WorkSchedules
                .Where(s => s.WorkDate.Month == now.Month && s.WorkDate.Year == now.Year && s.Status == "Completed")
                .Sum(s => s.TotalHours);

            decimal bonusRate = 1.0m;
            if (currentHours == maxHours && currentHours > 100) bonusRate = 1.15m;
            else if (currentHours > 100) bonusRate = 1.1m;

            decimal currentSalary = user.HourlyRate * (decimal)currentHours * bonusRate;
            grandTotal += currentSalary;

            var lastMonthUser = lastMonthList.FirstOrDefault(u => u.Id == user.Id);
            decimal lastMonthSalary = 0;
            if (lastMonthUser != null)
            {
                double prevHours = lastMonthUser.WorkSchedules
                    .Where(s => s.WorkDate.Month == lastMonth.Month && s.WorkDate.Year == lastMonth.Year && s.Status == "Completed")
                    .Sum(s => s.TotalHours);
                lastMonthSalary = lastMonthUser.HourlyRate * (decimal)prevHours;
            }

            displayList.Add(new UserSalaryInfo
            {
                User = user,
                TotalHours = currentHours,
                FinalSalary = currentSalary,
                LastMonthSalary = lastMonthSalary,
                BonusTag = bonusRate == 1.15m ? "TOP 1 (+15%)" : (bonusRate == 1.1m ? "+10%" : "0%"),
                TagColor = bonusRate == 1.15m ? new SolidColorBrush(Colors.Gold) :
                           (bonusRate == 1.1m ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.Gray))
            });
        }

        _allCalculatedData = displayList;
        var sortedList = ApplySorting(displayList);

        TotalPages = (int)Math.Ceiling((decimal)sortedList.Count / PageSize);
        if (CurrentPageNumber > TotalPages && TotalPages > 0)
            CurrentPageNumber = TotalPages;

        int skip = (CurrentPageNumber - 1) * PageSize;
        var pagedList = sortedList.Skip(skip).Take(PageSize).ToList();
        DisplayEmployees = new ObservableCollection<UserSalaryInfo>(pagedList);
        PageInfoDisplay = $"Page {CurrentPageNumber} / {TotalPages}";

        TotalPayrollDisplay = string.Format("{0:N0} đ", grandTotal);
        ActiveStaffCount = currentMonthList.Count;

        var topStaff = displayList.OrderByDescending(x => x.TotalHours).FirstOrDefault(x => x.TotalHours == (displayList.Max(d => d.TotalHours)) && x.TotalHours > 0);
        TopPerformerName = topStaff != null ? $"{topStaff.User.FullName} ({topStaff.TotalHours}h)" : "None";
    }

    [RelayCommand]
    public async Task UpdateUserStatusAsync(UserSalaryInfo info)
    {
        if (info?.User == null) return;

        //  Dùng UpdateEmployeeAsync (async method mới)
        await _employeeService.UpdateEmployeeAsync(info.User);

        ActiveStaffCount = DisplayEmployees.Count(x => x.User.IsActive);
        System.Diagnostics.Debug.WriteLine($"Updated status for {info.User.FullName}");
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
                // Lấy User đã được xử lý từ Dialog
                var newUser = dialog.SavedUser;

                if (newUser != null)
                {
                    await _employeeService.AddEmployeeAsync(newUser);

                    // Refresh lại bảng danh sách
                    await RefreshDashboardCommand.ExecuteAsync(null);

                    System.Diagnostics.Debug.WriteLine($"Thêm thành công: {newUser.FullName}");
                    _notificationService.ShowSuccess($"Thêm nhân viên '{newUser.FullName}' thành công");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi lưu: {ex.Message}");
                _notificationService.ShowError("Không thể thêm nhân viên. Vui lòng kiểm tra lại!");
                // Hiển thị thông báo lỗi cho người dùng thấy
                await _dialogService.ShowMessageAsync("Lỗi", "Không thể lưu nhân viên. Vui lòng kiểm tra lại!");
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
                var updatedUser = dialog.SavedUser;

                if (updatedUser != null)
                {
                    await _employeeService.UpdateEmployeeAsync(updatedUser);

                    await RefreshDashboardCommand.ExecuteAsync(null);

                    System.Diagnostics.Debug.WriteLine($"Cập nhật thành công: {updatedUser.FullName}");
                    _notificationService.ShowSuccess($"Cập nhật nhân viên '{updatedUser.FullName}' thành công");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi cập nhật: {ex.Message}");
                _notificationService.ShowError("Không thể cập nhật nhân viên. Vui lòng kiểm tra lại!");
                await _dialogService.ShowMessageAsync("Lỗi", "Không thể cập nhật nhân viên. Vui lòng kiểm tra lại!");
            }
        }
    }

    [RelayCommand]
    public async Task DeleteEmployeeAsync(UserSalaryInfo info)
    {
        if (info?.User == null) return;

        // Lấy XamlRoot an toàn từ App đang chạy
        var xamlRoot = ((App)Application.Current).MainWindow.Content.XamlRoot;

        var confirmDialog = new ContentDialog
        {
            Title = "Xác nhận xóa nhân viên",
            Content = $"Dữ liệu của '{info.User.FullName}' sẽ bị xóa vĩnh viễn. Sếp có chắc chắn không?",
            PrimaryButtonText = "Xóa ngay",
            SecondaryButtonText = "Hủy",
            DefaultButton = ContentDialogButton.Secondary,
            XamlRoot = xamlRoot // Đã fix lỗi CS0120
        };

        var result = await confirmDialog.ShowAsync();
        if (result != ContentDialogResult.Primary) return;

        try
        {
            await _employeeService.DeleteEmployeeAsync(info.User.Id);
            await RefreshDashboardAsync();
            System.Diagnostics.Debug.WriteLine($" Xoá thành công: {info.User.FullName}");
            _notificationService.ShowSuccess($"Xóa nhân viên '{info.User.FullName}' thành công");
        }
        catch (Exception ex)
        {
            _notificationService.ShowError("Không thể xóa nhân viên. Vui lòng thử lại!");
            await _dialogService.ShowMessageAsync("Lỗi", "Không thể xóa. Vui lòng thử lại!");
        }
    }

    [RelayCommand]
    public void NextPageAsync()
    {
        if (CurrentPageNumber < TotalPages)
        {
            CurrentPageNumber++;
            UpdateDisplayPage();
        }
    }

    [RelayCommand]
    public void PreviousPageAsync()
    {
        if (CurrentPageNumber > 1)
        {
            CurrentPageNumber--;
            UpdateDisplayPage();
        }
    }

    private void UpdateDisplayPage()
    {
        var sortedList = ApplySorting(_allCalculatedData);
        int skip = (CurrentPageNumber - 1) * PageSize;
        var pagedList = sortedList.Skip(skip).Take(PageSize).ToList();
        DisplayEmployees = new ObservableCollection<UserSalaryInfo>(pagedList);
        PageInfoDisplay = $"Page {CurrentPageNumber} / {TotalPages}";
    }

    [RelayCommand]
    public void SortByColumn(string columnName)
    {
        if (string.IsNullOrEmpty(columnName)) return;

        // Nếu click vào cột đã được sort, thay đổi hướng
        if (SortColumnName == columnName)
        {
            SortAscending = !SortAscending;
        }
        else
        {
            // Cột mới -> mặc định sort descending
            SortColumnName = columnName;
            SortAscending = false;
        }

        // Reset về trang 1 khi sort
        CurrentPageNumber = 1;
        ApplyFilter();
    }

    private List<UserSalaryInfo> ApplySorting(List<UserSalaryInfo> data)
    {
        var sorted = SortColumnName switch
        {
            "FullName" => SortAscending
                ? data.OrderBy(x => x.User.FullName).ToList()
                : data.OrderByDescending(x => x.User.FullName).ToList(),

            "Phone" => SortAscending
                ? data.OrderBy(x => x.User.Phone).ToList()
                : data.OrderByDescending(x => x.User.Phone).ToList(),

            "Role" => SortAscending
                ? data.OrderBy(x => x.User.Role).ToList()
                : data.OrderByDescending(x => x.User.Role).ToList(),

            "Status" => SortAscending
                ? data.OrderBy(x => x.User.IsActive).ToList()
                : data.OrderByDescending(x => x.User.IsActive).ToList(),

            "HourlyRate" => SortAscending
                ? data.OrderBy(x => x.User.HourlyRate).ToList()
                : data.OrderByDescending(x => x.User.HourlyRate).ToList(),

            "TotalHours" => SortAscending
                ? data.OrderBy(x => x.TotalHours).ToList()
                : data.OrderByDescending(x => x.TotalHours).ToList(),

            "LastMonthSalary" => SortAscending
                ? data.OrderBy(x => x.LastMonthSalary).ToList()
                : data.OrderByDescending(x => x.LastMonthSalary).ToList(),

            "FinalSalary" => SortAscending
                ? data.OrderBy(x => x.FinalSalary).ToList()
                : data.OrderByDescending(x => x.FinalSalary).ToList(),

            _ => data
        };

        return sorted;
    }

    public string GetSortIndicator(string columnName)
    {
        if (SortColumnName != columnName)
            return string.Empty;

        // \uE110 for ascending, \uE1FD for descending
        return SortAscending ? "\uE110" : "\uE1FD";
    }

}

// Class chứa dữ liệu hiển thị trên bảng
public class UserSalaryInfo
{
    public User User { get; set; } = null!;
    public double TotalHours { get; set; }
    public decimal FinalSalary { get; set; }
    public decimal LastMonthSalary { get; set; } // Lương tháng trước
    public string BonusTag { get; set; } = "0%";
    public SolidColorBrush TagColor { get; set; } = null!;
}