using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;
using MyShop.Views.CustomerManagement.Components;
using System.Diagnostics.CodeAnalysis;
using MyShop.Services; // Cần để lấy ICustomerService từ Container
using MyShop.Di;
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
    
    var customerService = ServiceContainer.Instance.Get<ICustomerService>();

  
    var viewModel = new CustomerManagementViewModel(
        customerService, // Quan trọng nhất là dòng này!
        onAdd: () => ShowFormDialog(isEditMode: false),
        onRules: NavigateToRules,
        onProfile: NavigateToProfile,
        onEdit: customer => ShowFormDialog(isEditMode: true, customer)
    );

    // 3. Navigate
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
