using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;

namespace MyShop.Views.StaffManagement.Components;

public sealed partial class EmployeeFormView : ContentDialog
{
    public EmployeeFormView(EmployeeFormViewModel viewModel)
    {
        this.InitializeComponent();
        this.DataContext = viewModel;
    }
    // Bind button click events to ViewModel commands
    private void CancelButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (this.DataContext is EmployeeFormViewModel vm && vm.CancelCommand.CanExecute(null))
        {
            vm.CancelCommand.Execute(null);
        }

        this.Hide();
    }

    private void SaveButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (this.DataContext is EmployeeFormViewModel vm && vm.SubmitCommand.CanExecute(null))
        {
            vm.SubmitCommand.Execute(null);
        }

        this.Hide();
    }
}
