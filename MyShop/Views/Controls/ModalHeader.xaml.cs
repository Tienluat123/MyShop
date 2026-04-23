using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MyShop.Views.Controls
{
    public sealed partial class ModalHeader : UserControl
    {
        public event RoutedEventHandler? CancelClicked;
        // 1. Khai báo thuộc tính Title
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(ModalHeader), new PropertyMetadata(string.Empty));

        // 2. Khai báo thuộc tính Subtitle
        public string Subtitle
        {
            get => (string)GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }
        public static readonly DependencyProperty SubtitleProperty =
            DependencyProperty.Register("Subtitle", typeof(string), typeof(ModalHeader), new PropertyMetadata(string.Empty));

        public ModalHeader()
        {
            this.InitializeComponent();
        }

        // 3. Hàm xử lý khi bấm nút X
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Khi bấm nút X, mình "bắn" cái sự kiện này ra ngoài
            CancelClicked?.Invoke(this, e);

        }
    }
}