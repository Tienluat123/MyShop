using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.ViewModels;

namespace MyShop.Views.CustomerManagement.Components;

public sealed partial class CustomerManagementView : Page
{
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
}