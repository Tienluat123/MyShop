using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MyShop.Views.StaffManagement.Dialogs;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;
using MyShop.Services;
using MyShop.Helpers;

namespace MyShop.ViewModels.StaffManagement;

public partial class WeeklyRosterViewModel : ObservableObject
{
    private readonly IScheduleService _scheduleService;
    private readonly IEmployeeService _employeeService;
    private readonly INotificationService _notificationService;

    // --- PROPERTIES (Dữ liệu hiển thị) ---

    [ObservableProperty]
    private DateOnly _currentWeekStart;

    [ObservableProperty]
    private bool _isLoading;

    // 3 hàng tương ứng 3 ca, mỗi hàng luôn có 7 ô (để Binding vào UniformGrid)
    public ObservableCollection<List<WorkSchedule?>> MorningShifts { get; } = new();
    public ObservableCollection<List<WorkSchedule?>> AfternoonShifts { get; } = new();
    public ObservableCollection<List<WorkSchedule?>> EveningShifts { get; } = new();

    // Đổi User? thành SelectStaffDialog?
    public Func<List<User>, Task<SelectStaffDialog?>>? ShowSelectStaffDialogAsync { get; set; }
    public Func<SelectStaffDialog, Task>? ShowEditStaffDialogAsync { get; set; }
    public WeeklyRosterViewModel(IScheduleService scheduleService, IEmployeeService employeeService, INotificationService notificationService)
    {
        _scheduleService = scheduleService;
        _employeeService = employeeService;
        _notificationService = notificationService;

        // Khởi tạo ngày bắt đầu là Thứ 2 tuần này
        CurrentWeekStart = DateHelper.GetFirstDayOfWeek(DateTime.Today);

        // Initialize với 7 empty lists (một cho mỗi ngày trong tuần)
        for (int i = 0; i < 7; i++)
        {
            MorningShifts.Add(new List<WorkSchedule?>());
            AfternoonShifts.Add(new List<WorkSchedule?>());
            EveningShifts.Add(new List<WorkSchedule?>());
        }
    }

    // --- COMMANDS (Hành động) ---

    /// <summary>
    /// Lệnh tải dữ liệu tuần hiện tại
    /// </summary>
    [RelayCommand]
    public async Task LoadSchedulesAsync()
    {
        if (IsLoading) return;
        IsLoading = true;

        try
        {
            var allData = await _scheduleService.GetRostersForWeek(CurrentWeekStart);
            PopulateRosterGrid(allData ?? new());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadSchedules error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Lệnh chuyển sang tuần tiếp theo
    /// </summary>
    [RelayCommand]
    private async Task NextWeekAsync()
    {
        CurrentWeekStart = CurrentWeekStart.AddDays(7);
        // Tải lại dữ liệu cho tuần mới
        await LoadSchedulesAsync();
    }

    /// <summary>
    /// Lệnh quay lại tuần trước
    /// </summary>
    [RelayCommand]
    private async Task PreviousWeekAsync()
    {
        CurrentWeekStart = CurrentWeekStart.AddDays(-7);
        await LoadSchedulesAsync();
    }

    [RelayCommand]
    public async Task AddStaff(object parameter)
    {
        if (parameter is not WorkSchedule targetSlot) return;
        if (ShowSelectStaffDialogAsync is null) return;

        var allStaff = await _employeeService.GetActiveEmployeesAsync();
        var dialog = await ShowSelectStaffDialogAsync.Invoke(allStaff);

        if (dialog?.SelectedUser != null)
        {
            // Check if user already scheduled on that date
            var isDuplicate = await _scheduleService.IsUserAlreadyScheduledOnDateAsync(
                dialog.SelectedUser.Id,
                targetSlot.WorkDate
            );

            if (isDuplicate)
            {
                System.Diagnostics.Debug.WriteLine($"Duplicate: {dialog.SelectedUser.FullName} already scheduled on {targetSlot.WorkDate}");
                return; // Block duplicate
            }

            var newSchedule = new WorkSchedule
            {
                UserId = dialog.SelectedUser.Id,
                WorkDate = targetSlot.WorkDate,
                ShiftName = targetSlot.ShiftName,
                Note = dialog.SelectedNote,
                Status = dialog.SelectedStatus
            };

            try
            {
                var result = await _scheduleService.CreateAsync(newSchedule);
                if (result)
                {
                    _notificationService.ShowSuccess($"Thêm {dialog.SelectedUser.FullName} vào lịch thành công");
                    await LoadSchedulesCommand.ExecuteAsync(null);
                }
            }
            catch (Exception)
            {
                _notificationService.ShowError("Không thể thêm vào lịch. Vui lòng thử lại!");
            }
        }
    }

    [RelayCommand]
    public async Task EditStaff(WorkSchedule targetSchedule)
    {
        if (targetSchedule == null || targetSchedule.Id == 0) return;
        var allStaff = await _employeeService.GetActiveEmployeesAsync();
        // Khởi tạo Dialog với dữ liệu cũ
        var dialog = new SelectStaffDialog(allStaff, targetSchedule.User, targetSchedule.Status, targetSchedule.Note);

        if (ShowEditStaffDialogAsync != null)
        {
            // CHỈ GỌI QUA MỎ NEO (Mỏ neo này trong Code-behind sẽ lo việc gán XamlRoot và ShowAsync)
            await ShowEditStaffDialogAsync.Invoke(dialog);
            // 3. Sau khi Dialog đóng (Hide), kiểm tra Result
            if (dialog.IsDeleteRequested)
            {
                // Xóa nếu tồn tại
                if (targetSchedule.Id > 0)
                {
                    try
                    {
                        await _scheduleService.DeleteAsync(targetSchedule.Id);
                        _notificationService.ShowSuccess($"Xóa {targetSchedule.User?.FullName} khỏi lịch thành công");
                    }
                    catch (Exception)
                    {
                        _notificationService.ShowError("Không thể xóa khỏi lịch. Vui lòng thử lại!");
                    }
                }
            }
            else if (dialog.Result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                if (dialog.SelectedUser is null)
                {
                    _notificationService.ShowError("Vui lòng chọn nhân viên trước khi lưu.");
                    return;
                }

                // Cập nhật thông tin
                targetSchedule.UserId = dialog.SelectedUser.Id;
                targetSchedule.Note = dialog.SelectedNote;
                targetSchedule.Status = dialog.SelectedStatus;

                try
                {
                    await _scheduleService.UpdateAsync(targetSchedule);
                    _notificationService.ShowSuccess($"Cập nhật lịch cho {dialog.SelectedUser.FullName} thành công");
                }
                catch (Exception)
                {
                    _notificationService.ShowError("Không thể cập nhật lịch. Vui lòng thử lại!");
                }
            }

            // Load lại lịch để cập nhật giao diện
            await LoadSchedulesAsync();
        }
    }

    // --- PRIVATE LOGIC (Xử lý ngầm) ---
    private List<WorkSchedule> GetFiveSlots(List<WorkSchedule> allData, DateOnly date, string shiftName)
    {
        // Keep original order from database - no sorting
        var actual = allData
            .Where(s => s.WorkDate == date && s.ShiftName == shiftName)
            .ToList();

        var slots = new List<WorkSchedule>();

        for (int j = 0; j < 5; j++)
        {
            if (j < actual.Count)
            {
                slots.Add(actual[j]);
            }
            else
            {
                // Placeholder object for empty slot - has date/shift but no UserId
                slots.Add(new WorkSchedule
                {
                    Id = 0, // Indicates placeholder
                    UserId = 0,
                    WorkDate = date,
                    ShiftName = shiftName,
                    User = null
                });
            }
        }
        return slots;
    }

    private void PopulateRosterGrid(List<WorkSchedule> allData)
    {
        // Không Clear! Thay vào đó, replace items tại từng index để giữ collection count = 7
        for (int i = 0; i < 7; i++)
        {
            var date = CurrentWeekStart.AddDays(i);

            // Nạp đủ 5 suất cho từng ca
            var morning = GetFiveSlots(allData, date, "Morning");
            var afternoon = GetFiveSlots(allData, date, "Afternoon");
            var evening = GetFiveSlots(allData, date, "Evening");

            // Replace items tại index i (không Clear collection)
            MorningShifts[i] = morning as List<WorkSchedule?>;
            AfternoonShifts[i] = afternoon as List<WorkSchedule?>;
            EveningShifts[i] = evening as List<WorkSchedule?>;
        }
    }

}