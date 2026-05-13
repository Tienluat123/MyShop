using System;
using System.Threading.Tasks;
using MyShop.Models;
using MyShop.Services;
using MyShop.ViewModels.StaffManagement;
using MyShop.Views.StaffManagement.Dialogs;

namespace MyShop.Views.StaffManagement.Components;

internal sealed class EmployeeFormDialogHandler
{
    private readonly IEmployeeService _employeeService;
    private readonly IDialogService _dialogService;
    private readonly INotificationService _notificationService;

    public EmployeeFormDialogHandler(
        IEmployeeService employeeService,
        IDialogService dialogService,
        INotificationService notificationService)
    {
        _employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    public async Task HandleAddAsync(Func<Task> refreshAsync)
    {
        var formViewModel = new EmployeeFormViewModel();
        var dialog = new EmployeeFormDialog(formViewModel);
        dialog.SetEmployee(null);

        await _dialogService.ShowAsync(dialog);

        if (dialog.SavedUser is null)
        {
            return;
        }

        try
        {
            await _employeeService.AddEmployeeAsync(dialog.SavedUser);
            await refreshAsync();
            _notificationService.ShowSuccess($"Added employee '{dialog.SavedUser.FullName}' successfully");
        }
        catch
        {
            _notificationService.ShowError("Unable to add employee");
        }
    }

    public async Task HandleEditAsync(UserSalaryInfo info, Func<Task> refreshAsync)
    {
        if (info.User is null)
        {
            return;
        }

        var formViewModel = new EmployeeFormViewModel();
        var dialog = new EmployeeFormDialog(formViewModel);
        dialog.SetEmployee(info.User);

        await _dialogService.ShowAsync(dialog);

        if (dialog.SavedUser is null)
        {
            return;
        }

        try
        {
            await _employeeService.UpdateEmployeeAsync(dialog.SavedUser);
            await refreshAsync();
            _notificationService.ShowSuccess($"Updated '{dialog.SavedUser.FullName}' successfully");
        }
        catch
        {
            _notificationService.ShowError("Unable to update employee");
        }
    }
}