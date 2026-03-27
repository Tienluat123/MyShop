namespace MyShop.Views.StaffManagement.Components;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;

public sealed partial class EmployeeListTab : UserControl
{
    private EmployeeListViewModel _viewModel;

    public EmployeeListTab()
    {
        this.InitializeComponent();
        _viewModel = new EmployeeListViewModel();
        this.DataContext = _viewModel;

        // Wire up search textbox
        SearchTextBox.TextChanged += SearchTextBox_TextChanged;
    }

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        _viewModel.SearchText = SearchTextBox.Text;
        _viewModel.SearchCommand.Execute(null);
    }

    private async void AddEmployee_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = new EmployeeFormViewModel(isEditMode: false, employee: null, onCancel: () => { });
        var dialog = new EmployeeFormView(viewModel)
        {
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            // Employee data is available in viewModel.EmployeeData
            // Save it to your data source
        }
    }

    private async void EditEmployee_Click(object sender, RoutedEventArgs e)
    {
        // Get the employee from the button's Tag
        var button = sender as Button;
        var employee = button?.Tag as EmployeeListItem;

        if (employee == null)
            return;

        // Create employee data object from existing data
        var employeeData = new EmployeeData
        {
            Name = employee.Name,
            Phone = employee.Phone,
            Position = 0, // Admin - you might want to map this properly
            Status = 0    // Đang làm việc - you might want to map this properly
        };

        var viewModel = new EmployeeFormViewModel(isEditMode: true, employee: employeeData, onCancel: () => { });
        var dialog = new EmployeeFormView(viewModel)
        {
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            // Employee data updated in viewModel.EmployeeData
            // Save changes to your data source
        }
    }
}
