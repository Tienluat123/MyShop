using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;

namespace MyShop.Views.StaffManagement.Components;

public sealed partial class EmployeeListTab : UserControl
{
    public EmployeeListTab()
    {
        InitializeComponent();
    }

    public void Initialize(EmployeeListViewModel viewModel)
    {
        DataContext = viewModel;
    }
}
