using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using MyShop.Models;
using MyShop.Services;
using MyShop.ViewModels;
using MyShop.Views.CustomerManagement.Components;
using System.Diagnostics;
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
        Debug.WriteLine("[CustomerPage] Loaded event fired. Frame.Content is null: " + (CustomerContentFrame.Content is null));
        if (CustomerContentFrame.Content is null)
        {
            Debug.WriteLine("[CustomerPage] Navigating to ManagementView...");
            NavigateToManagement();
        }
    }

    private void NavigateToManagement()
    {
        var viewModel = new CustomerManagementViewModel(
            onAdd: () => ShowFormDialog(isEditMode: false),
            onRules: NavigateToRules,
            onProfile: NavigateToProfile,
            onEdit: customer => ShowFormDialog(isEditMode: true, customer));

        CustomerContentFrame.Navigate(typeof(CustomerManagementView), viewModel);
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    private void LoyaltyRules_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        NavigateToRules();
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    private void NavigateToRules()
    {
        Debug.WriteLine("[CustomerPage] NavigateToRules called.");
        var loyaltyService = App.Services.GetRequiredService<ILoyaltyService>();
        Debug.WriteLine($"[CustomerPage] LoyaltyService resolved: {loyaltyService != null}");
        var viewModel = new LoyaltyRulesViewModel(loyaltyService, onBack: NavigateToManagement);
        Debug.WriteLine("[CustomerPage] LoyaltyRulesViewModel created, now navigating...");
        CustomerContentFrame.Navigate(typeof(LoyaltyRulesView), viewModel);
        Debug.WriteLine("[CustomerPage] Navigate called.");
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    private void NavigateToProfile(CustomerListItem customer)
    {
        var viewModel = new CustomerProfileViewModel(customer, onBack: NavigateToManagement);
        CustomerContentFrame.Navigate(typeof(CustomerProfileView), viewModel);
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    private async void ShowFormDialog(bool isEditMode, CustomerListItem? customer = null)
    {
        var viewModel = new CustomerFormViewModel(isEditMode, customer, onCancel: () => { });
        var dialog = new CustomerFormView(viewModel)
        {
            XamlRoot = this.XamlRoot
        };
        await dialog.ShowAsync();
    }
}
