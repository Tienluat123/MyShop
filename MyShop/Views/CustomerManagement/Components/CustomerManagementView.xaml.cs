using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.ViewModels;

namespace MyShop.Views.CustomerManagement.Components;

public sealed partial class CustomerManagementView : Page
{
    private bool _isStatusUpdating = false;

    public CustomerManagementView()
    {
        this.InitializeComponent();

        // Đăng ký sự kiện Loaded: Khi giao diện vẽ xong hoàn toàn mới cho phép load data
        this.Loaded += CustomerManagementView_Loaded;
    }

    private CustomerManagementViewModel? ViewModel => DataContext as CustomerManagementViewModel;

    private void CustomerManagementView_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Kiểm tra nếu ViewModel đã có (đã được set từ OnNavigatedTo)
        // thì mới ra lệnh nạp 6 khách hàng
        if (ViewModel != null)
        {
            ViewModel.LoadCustomersCommand.Execute(null);
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        // Nhận ViewModel từ trang cha truyền sang
        DataContext = e.Parameter as CustomerManagementViewModel;
    }
    private async void DeleteButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // 1. Lấy khách hàng từ dòng vừa nhấn
        var button = (Button)sender;
        var customer = (CustomerListItem)button.DataContext;

        // 2. Tạo Dialog xác nhận
        ContentDialog deleteDialog = new ContentDialog
        {
            Title = "Confirm Deletion",
            Content = $"Are you sure you want to delete {customer.Name}? This action cannot be undone.",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.XamlRoot // Bắt buộc trong WinUI 3
        };

        // 3. Nếu người dùng nhấn Delete
        var result = await deleteDialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            // Gọi lệnh xóa trong ViewModel
            ViewModel?.DeleteCustomerCommand.Execute(customer);
        }
    }

    private void ViewProfileButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var button = (Button)sender;
        var customer = (CustomerListItem)button.DataContext;
        ViewModel?.MapsToProfileCommand.Execute(customer);
    }

    private void EditButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var button = (Button)sender;
        var customer = (CustomerListItem)button.DataContext;
        ViewModel?.EditCustomerCommand.Execute(customer);
    }

    private void PreviousButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ViewModel?.PreviousPageCommand.Execute(null);
    }

    private void NextButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ViewModel?.NextPageCommand.Execute(null);
    }

    private async void Status_Toggled(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Prevent rapid-fire toggle events from exhausting connection pool
        if (_isStatusUpdating) return;
        _isStatusUpdating = true;

        try
        {
            var toggleSwitch = (ToggleSwitch)sender;
            var customer = (CustomerListItem?)toggleSwitch.DataContext;

            if (customer != null)
            {
                customer.Status = toggleSwitch.IsOn ? "Active" : "Inactive";
                if (ViewModel?.LoadCustomersCommand.CanExecute(null) ?? false)
                {
                    ViewModel.LoadCustomersCommand.Execute(null);
                }
            }
        }
        finally
        {
            _isStatusUpdating = false;
        }
    }
}