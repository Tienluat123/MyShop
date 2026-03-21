using Microsoft.UI.Xaml.Controls;

namespace MyShop.Views.StaffManagement;

public sealed partial class StaffSchedulePage : Page
{
    public StaffSchedulePage()
    {
        this.InitializeComponent();
    }

    private void Tab_Checked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Đề phòng lỗi lúc app mới khởi động lên chưa load kịp giao diện
        if (WeeklyContent == null || ListContent == null) return;

        if (TabWeekly.IsChecked == true)
        {
            // Nếu Tab 1 được chọn -> Bật Component Lịch, Tắt Component Danh sách
            WeeklyContent.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            ListContent.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        }
        else
        {
            // Nếu Tab 2 được chọn -> Bật Danh sách, Tắt Lịch
            WeeklyContent.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            ListContent.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        }
    }
}