using Microsoft.UI.Xaml;
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

    private void CustomerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel is not null && sender is ListView listView && listView.SelectedItem is CustomerListItem selected)
        {
            ViewModel.SelectedCustomer = selected;
        }
    }

    private void CustomerList_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (ViewModel is not null && e.ClickedItem is CustomerListItem customer)
        {
            ViewModel.OpenProfile(customer);
        }
    }

    private void OpenProfileButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel is not null && sender is Button button && button.Tag is CustomerListItem customer)
        {
            ViewModel.OpenProfile(customer);
        }
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel is not null && sender is Button button && button.Tag is CustomerListItem customer)
        {
            ViewModel.EditCustomer(customer);
        }
    }
}
