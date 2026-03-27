using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.ViewModels;

namespace MyShop.Views.CustomerManagement.Components;

public sealed partial class CustomerManagementView : Page
{
    public CustomerManagementView()
    {
        InitializeComponent();
    }

    private CustomerManagementViewModel? ViewModel => DataContext as CustomerManagementViewModel;

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        DataContext = e.Parameter as CustomerManagementViewModel;
    }
}
