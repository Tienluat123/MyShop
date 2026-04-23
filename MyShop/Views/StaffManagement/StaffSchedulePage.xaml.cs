using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Text;
using Windows.UI;
using System;

namespace MyShop.Views.StaffManagement;

public sealed partial class StaffSchedulePage : Page
{
    // Chuẩn bị sẵn các màu sắc để tái sử dụng
    private readonly SolidColorBrush _orangeBrush = new(Color.FromArgb(255, 217, 119, 6));
    private readonly SolidColorBrush _transparentBrush = new(Colors.Transparent);
    private readonly SolidColorBrush _whiteBrush = new(Colors.White);
    private readonly SolidColorBrush _darkTextBrush = new(Color.FromArgb(255, 55, 65, 81));
    private readonly SolidColorBrush _borderGrayBrush = new(Color.FromArgb(255, 209, 213, 219));

    public StaffSchedulePage()
    {
        this.InitializeComponent();
    }

    private void NavButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button clickedButton && clickedButton.Tag != null)
        {
            string target = clickedButton.Tag?.ToString() ?? string.Empty;

            // 1. Reset tất cả về trạng thái bình thường (Chưa chọn)
            ResetButtonStyles();

            // 2. Highlight nút vừa được nhấn (Màu cam)
            clickedButton.Background = _orangeBrush;
            clickedButton.Foreground = _whiteBrush;
            clickedButton.BorderBrush = _orangeBrush;
            clickedButton.FontWeight = FontWeights.SemiBold;

            // 3. Thực hiện chuyển đổi nội dung hiển thị
            WeeklyContent.Visibility = (target == "weekly") ? Visibility.Visible : Visibility.Collapsed;
            ListContent.Visibility = (target == "list") ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void ResetButtonStyles()
    {
        // Đưa nút Weekly về trạng thái không chọn
        BtnWeekly.Background = _transparentBrush;
        BtnWeekly.Foreground = _darkTextBrush;
        BtnWeekly.BorderBrush = _borderGrayBrush;
        BtnWeekly.FontWeight = FontWeights.Normal;

        // Đưa nút List về trạng thái không chọn
        BtnList.Background = _transparentBrush;
        BtnList.Foreground = _darkTextBrush;
        BtnList.BorderBrush = _borderGrayBrush;
        BtnList.FontWeight = FontWeights.Normal;
    }
}