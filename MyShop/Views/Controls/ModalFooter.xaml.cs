using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MyShop.Views.Controls;

public sealed partial class ModalFooter : UserControl
{
    // ==========================================
    // 1. NHẬN CHỮ TRUYỀN VÀO (DEPENDENCY PROPERTIES)
    // ==========================================

    public string CancelText
    {
        get => (string)GetValue(CancelTextProperty);
        set => SetValue(CancelTextProperty, value);
    }
    public static readonly DependencyProperty CancelTextProperty =
        DependencyProperty.Register("CancelText", typeof(string), typeof(ModalFooter), new PropertyMetadata("Cancel"));

    public string SubmitText
    {
        get => (string)GetValue(SubmitTextProperty);
        set => SetValue(SubmitTextProperty, value);
    }
    public static readonly DependencyProperty SubmitTextProperty =
        DependencyProperty.Register("SubmitText", typeof(string), typeof(ModalFooter), new PropertyMetadata("Submit"));


    // ==========================================
    // 2. TẠO CÁC CÁP KẾT NỐI (EVENTS) ĐỂ BÁO RA NGOÀI
    // ==========================================

    public event RoutedEventHandler CancelClicked;
    public event RoutedEventHandler SubmitClicked;


    // ==========================================
    // 3. KHỞI TẠO & XỬ LÝ CLICK BÊN TRONG COMPONENT
    // ==========================================

    public ModalFooter()
    {
        this.InitializeComponent();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        // Bắn tín hiệu ra ngoài cho thằng Form cha biết là nút Hủy đã bị bấm
        CancelClicked?.Invoke(this, e);
    }

    private void SubmitButton_Click(object sender, RoutedEventArgs e)
    {
        // Bắn tín hiệu ra ngoài cho thằng Form cha biết là nút Xác nhận đã bị bấm
        SubmitClicked?.Invoke(this, e);
    }
}