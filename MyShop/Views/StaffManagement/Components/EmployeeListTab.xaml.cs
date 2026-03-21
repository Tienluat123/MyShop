namespace MyShop.Views.StaffManagement.Components;

public sealed partial class EmployeeListTab : UserControl
{
    public EmployeeListTab()
    {
        this.InitializeComponent();
    }

    private async void AddEmployee_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // 1. Khởi tạo cái Dialog ông vừa tách ra
        var dialog = new Components.EmployeeFormDialog();

        // 2. BẮT BUỘC TRONG WINUI 3: Nói cho nó biết nó đang nổi lên ở cái cửa sổ nào
        dialog.XamlRoot = this.XamlRoot;

        // 3. Mở nó lên và đợi người dùng bấm nút
        var result = await dialog.ShowAsync();

        // 4. Bắt sự kiện người dùng bấm Lưu hay Hủy
        if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
        {
            // Người ta vừa bấm nút "Lưu thông tin" (PrimaryButton)
            // Lát nữa mình viết code thêm data vào danh sách ở đây!
        }
    }

    private async void EditEmployee_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var dialog = new Components.EmployeeFormDialog();
        dialog.XamlRoot = this.XamlRoot;

        // 1. Tiêu đề
        dialog.Title = "Chỉnh sửa thông tin nhân viên";
        dialog.PrimaryButtonText = "Cập nhật thay đổi";

        // 2. Bơm dữ liệu cũ của Nguyễn Văn Nam vào form!
        // (0 = Admin, 0 = Đang làm việc)
        dialog.LoadDuLieuCu("Nguyễn Văn Nam", "0912345678", 0, 0);

        // 3. Mở popup lên (Lúc này form đã có sẵn chữ bên trong)
        var result = await dialog.ShowAsync();

        if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
        {
            // Bấm Cập nhật -> Gọi code lưu vào Database
        }
    }
}
