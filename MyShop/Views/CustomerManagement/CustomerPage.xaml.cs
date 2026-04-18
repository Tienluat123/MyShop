using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using MyShop.Models;
using MyShop.Services;
using MyShop.ViewModels;
using MyShop.Views.CustomerManagement.Components;
using System.Diagnostics;
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
        Debug.WriteLine("[CustomerPage] Loaded event fired. Frame.Content is null: " + (CustomerContentFrame.Content is null));
        if (CustomerContentFrame.Content is null)
        {
            Debug.WriteLine("[CustomerPage] Navigating to ManagementView...");
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
        Debug.WriteLine("[CustomerPage] NavigateToRules called.");
        var loyaltyService = App.Services.GetRequiredService<ILoyaltyService>();
        Debug.WriteLine($"[CustomerPage] LoyaltyService resolved: {loyaltyService != null}");
        var viewModel = new LoyaltyRulesViewModel(loyaltyService, onBack: NavigateToManagement);
        Debug.WriteLine("[CustomerPage] LoyaltyRulesViewModel created, now navigating...");
        CustomerContentFrame.Navigate(typeof(LoyaltyRulesView), viewModel);
        Debug.WriteLine("[CustomerPage] Navigate called.");
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    private void NavigateToProfile(CustomerListItem selectedCustomer)
{
    // 1. Lấy Service ra từ Container
    var customerService = ServiceContainer.Instance.Get<ICustomerService>();
    var orderService = ServiceContainer.Instance.Get<IOrderService>();

    // 2. Nạp đủ 4 món vào hàm khởi tạo
    var viewModel = new CustomerProfileViewModel(
        selectedCustomer, 
        customerService, 
        orderService, 
        onBack: () => NavigateToManagement() // Quay lại trang danh sách
    );

    // 3. Chuyển trang
    CustomerContentFrame.Navigate(typeof(CustomerProfileView), viewModel);
}

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    private async void ShowFormDialog(bool isEditMode, CustomerListItem? customer = null)
    {
        var customerService = ServiceContainer.Instance.Get<ICustomerService>();
        
        // Tiêm Service và truyền Action để Refresh danh sách khi lưu thành công
        var viewModel = new CustomerFormViewModel(
            customerService, 
            isEditMode, 
            customer, 
            onSuccess: () => {
                // Tải lại danh sách khách hàng ở trang chính
                NavigateToManagement(); 
            });

        var dialog = new CustomerFormView(viewModel) { XamlRoot = this.XamlRoot };
        await dialog.ShowAsync();
    }
}
