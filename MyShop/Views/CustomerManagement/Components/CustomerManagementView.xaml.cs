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

    private async void EditButton_Click(object sender, RoutedEventArgs e)
    {
        // Lấy được thông tin khách hàng từ cái Tag của nút bấm rồi nè
        if (ViewModel is not null && sender is Button button && button.Tag is CustomerListItem customer)
        {
            // 1. Khởi tạo ViewModel cho Form, nhưng lần này là bật mode EDIT
            var formViewModel = new CustomerFormViewModel(
                true,       // isEditMode = true (Báo cho Form biết đây là cập nhật)
                customer,   // Quăng nguyên cục data khách hàng vào để nó điền sẵn chữ lên ô TextBox
                () =>
                {
                    // Hành động sau khi lưu thành công (Ví dụ: Load lại danh sách)
                    // ViewModel.LoadCustomers(); 
                }
            );

            // 2. Mở Dialog lên y chang như lúc Add
            var dialog = new CustomerFormView(formViewModel);

            // Nhớ gắn XamlRoot để nó biết phải nổi lên ở đâu
            dialog.XamlRoot = this.XamlRoot;

            // 3. Bật dialog lên
            await dialog.ShowAsync();
        }
    }

    private async void AddCustomerButton_Click(object sender, RoutedEventArgs e)
    {
        // Truyền đủ 3 tham số mà Constructor yêu cầu:
        // 1. false: Báo cho Form biết đây là Thêm mới, không phải Edit.
        // 2. null: Vì thêm mới nên chưa có data khách hàng nào cả.
        // 3. () => { ... }: Cái hàm sẽ chạy khi người dùng bấm Lưu thành công.
        var formViewModel = new CustomerFormViewModel(
            false,
            null,
            () =>
            {
                // Chỗ này gọi hàm load lại danh sách từ ViewModel chính của trang
                // Ví dụ: ViewModel?.LoadCustomers(); 
                // Tạm thời có thể để trống {} nếu chưa viết hàm reload
            }
        );

        var dialog = new CustomerFormView(formViewModel);
        dialog.XamlRoot = this.XamlRoot;
        await dialog.ShowAsync();
    }
}
