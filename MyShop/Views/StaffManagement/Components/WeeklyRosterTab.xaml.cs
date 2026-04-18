using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels.StaffManagement;
using MyShop.Di;
using MyShop.Models;
using MyShop.Views.StaffManagement.Dialogs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyShop.Views.StaffManagement.Components;

public sealed partial class WeeklyRosterTab : UserControl
{
    public WeeklyRosterViewModel ViewModel { get; }

    public WeeklyRosterTab()
    {
        this.InitializeComponent();

        // 1. Khởi tạo ViewModel
        ViewModel = ServiceContainer.Instance.Get<WeeklyRosterViewModel>();
        this.DataContext = ViewModel;

        // 2. Setup callback để mở dialog
        ViewModel.ShowSelectStaffDialogAsync = ShowSelectStaffDialogAsync;

        ViewModel.ShowEditStaffDialogAsync = ShowEditStaffDialogAsync;

        // 3. Tự động load dữ liệu khi vào Tab
        this.Loaded += async (s, e) =>
        {
            if (ViewModel.LoadSchedulesCommand.CanExecute(null))
            {
                await ViewModel.LoadSchedulesCommand.ExecuteAsync(null);
            }
        };
    }

    private async Task ShowEditStaffDialogAsync(SelectStaffDialog dialog)
    {

        dialog.XamlRoot = this.XamlRoot;
        // Hiển thị và đợi người dùng thao tác xong
        await dialog.ShowAsync();
    }

    private async Task<SelectStaffDialog?> ShowSelectStaffDialogAsync(List<User> staffList)
    {
        var dialog = new SelectStaffDialog(staffList)
        {
            XamlRoot = this.XamlRoot
        };

        await dialog.ShowAsync();

        // Check dialog.Result instead of ShowAsync() return value
        if (dialog.Result == ContentDialogResult.Primary)
        {
            return dialog;
        }

        return null;
    }
}