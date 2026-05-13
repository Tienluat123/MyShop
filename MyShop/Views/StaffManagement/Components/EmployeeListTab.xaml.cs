using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels.StaffManagement;
using MyShop.Services;
using MyShop.Di;
using System;
using System.Threading.Tasks;

namespace MyShop.Views.StaffManagement.Components;

public sealed partial class EmployeeListTab : UserControl
{
    private EmployeeFormDialogHandler? _dialogHandler;

    public EmployeeManagementViewModel? ViewModel
    {
        get => (EmployeeManagementViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register("ViewModel", typeof(EmployeeManagementViewModel), typeof(EmployeeListTab), new PropertyMetadata(null));

    public EmployeeListTab()
    {
        this.InitializeComponent();
        this.Loaded += EmployeeListTab_Loaded;
    }

    private async void EmployeeListTab_Loaded(object sender, RoutedEventArgs e)
    {
        if (ViewModel == null)
        {
            var employeeService = ServiceContainer.Instance.Get<IEmployeeService>();
            var scheduleService = ServiceContainer.Instance.Get<IScheduleService>();
            var notificationService = ServiceContainer.Instance.Get<INotificationService>();
            var dialogService = ServiceContainer.Instance.Get<IDialogService>();
            var configService = ServiceContainer.Instance.Get<IAppConfigService>();

            _dialogHandler = new EmployeeFormDialogHandler(
                employeeService,
                dialogService,
                notificationService);

            ViewModel = new EmployeeManagementViewModel(
                employeeService,
                scheduleService,
                notificationService,
                configService,
                onAddRequested: ShowAddEmployeeDialogAsync,
                onEditRequested: ShowEditEmployeeDialogAsync);
        }

        if (ViewModel != null)
        {
            await ViewModel.RefreshCommand.ExecuteAsync(null);
        }
    }

    private async Task ShowAddEmployeeDialogAsync()
    {
        if (ViewModel is null || _dialogHandler is null)
        {
            return;
        }

        await _dialogHandler.HandleAddAsync(() => ViewModel.RefreshCommand.ExecuteAsync(null));
    }

    private async Task ShowEditEmployeeDialogAsync(UserSalaryInfo info)
    {
        if (ViewModel is null || _dialogHandler is null)
        {
            return;
        }

        await _dialogHandler.HandleEditAsync(info, () => ViewModel.RefreshCommand.ExecuteAsync(null));
    }

    private void Status_Toggled(object sender, RoutedEventArgs e)
    {
        if (ViewModel?.IsLoading == true)
        {
            return;
        }

        if (sender is ToggleSwitch toggle && toggle.DataContext is UserSalaryInfo info && ViewModel != null)
        {
            _ = ViewModel.UpdateUserStatusAsync(info);
        }
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is UserSalaryInfo info && ViewModel != null)
        {
            _ = ViewModel.EditEmployeeAsync(info);
        }
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is UserSalaryInfo info && ViewModel != null)
        {
            _ = ViewModel.DeleteEmployeeAsync(info);
        }
    }

    private void PreviousButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.PreviousPageCommand.Execute(null);
    }

    private void NextButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.NextPageCommand.Execute(null);
    }
}
