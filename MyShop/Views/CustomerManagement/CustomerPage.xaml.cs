using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;
using MyShop.Views.CustomerManagement.Components;
using System.Diagnostics.CodeAnalysis;

namespace MyShop.Views.CustomerManagement;

public sealed partial class CustomerPage : Page
{
    public CustomerPage()
    {
        InitializeComponent();
        Loaded += CustomerPage_Loaded;
    }

    private void CustomerPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (CustomerContentFrame.Content is null)
        {
            NavigateToManagement();
        }
    }

    private void NavigateToManagement()
    {
        var viewModel = new CustomerManagementViewModel(
            onAdd: () => NavigateToForm(isEditMode: false),
            onRules: NavigateToRules,
            onProfile: NavigateToProfile,
            onEdit: customer => NavigateToForm(isEditMode: true, customer));

        CustomerContentFrame.Navigate(typeof(CustomerManagementView), viewModel);
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    private void NavigateToRules()
    {
        var viewModel = new LoyaltyRulesViewModel(onBack: NavigateToManagement);
        CustomerContentFrame.Navigate(typeof(LoyaltyRulesView), viewModel);
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    private void NavigateToProfile(CustomerListItem customer)
    {
        var viewModel = new CustomerProfileViewModel(customer, onBack: NavigateToManagement);
        CustomerContentFrame.Navigate(typeof(CustomerProfileView), viewModel);
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    private void NavigateToForm(bool isEditMode, CustomerListItem? customer = null)
    {
        var viewModel = new CustomerFormViewModel(isEditMode, customer, onCancel: NavigateToManagement);
        CustomerContentFrame.Navigate(typeof(CustomerFormView), viewModel);
    }
}
