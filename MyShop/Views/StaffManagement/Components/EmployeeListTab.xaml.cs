using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels.StaffManagement;
using MyShop.Services;
using MyShop.Di;
using System;

namespace MyShop.Views.StaffManagement.Components;

public sealed partial class EmployeeListTab : UserControl
{
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
            ViewModel = ServiceContainer.Instance.Get<EmployeeManagementViewModel>();
        }

        if (ViewModel != null)
        {
            await ViewModel.RefreshCommand.ExecuteAsync(null);
        }
    }

    private void Status_Toggled(object sender, RoutedEventArgs e)
    {
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