using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;
using MyShop.Views.StaffManagement.Components;
using System.Diagnostics.CodeAnalysis;

namespace MyShop.Views.StaffManagement;

public sealed partial class StaffSchedulePage : Page
{
    public StaffSchedulePage()
    {
        this.InitializeComponent();
        InitializeStaffList();
    }

    private void InitializeStaffList()
    {
        var viewModel = new EmployeeListViewModel(
            onAdd: () => ShowEmployeeFormDialog(isEditMode: false),
            onEdit: employee => ShowEmployeeFormDialog(isEditMode: true, employee));
        
        ListContent.Initialize(viewModel);
    }

    private void Tab_Checked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (WeeklyContent == null || ListContent == null) return;

        if (TabWeekly.IsChecked == true)
        {
            WeeklyContent.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            ListContent.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        }
        else
        {
            WeeklyContent.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            ListContent.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        }
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    private async void ShowEmployeeFormDialog(bool isEditMode, EmployeeListItem? employee = null)
    {
        // Note: Staff module seems to use EmployeeData model for the form
        // We'll prepare it based on the list item if editing
        EmployeeData? initialData = null;
        if (isEditMode && employee != null)
        {
            initialData = new EmployeeData
            {
                Name = employee.Name,
                Phone = employee.Phone,
                Position = 0, // Placeholder mapping
                Status = 0    // Placeholder mapping
            };
        }

        var formViewModel = new EmployeeFormViewModel(isEditMode, initialData, onCancel: () => { });
        var dialog = new EmployeeFormDialog(formViewModel)
        {
            XamlRoot = this.XamlRoot
        };

        await dialog.ShowAsync();
        // Logical refresh would happen here if it was a real data source
    }
}