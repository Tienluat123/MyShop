using MyShop.Di;
using MyShop.Services;
using MyShop.ViewModels;
using MyShop.Views.Pos;

namespace MyShop.Views;

public sealed partial class LoginPage : Page
{
    private readonly LoginViewModel _viewModel;

    public LoginPage()
    {
        this.InitializeComponent();

        // Lấy AuthService và SessionService từ DI container
        var authService = ServiceContainer.Instance.Get<IAuthService>();
        var sessionService = ServiceContainer.Instance.Get<ISessionService>();
        _viewModel = new LoginViewModel(authService, sessionService);

        // Gắn DataContext để XAML Binding hoạt động
        this.DataContext = _viewModel;

        // Lắng nghe sự kiện đăng nhập thành công để điều hướng
        _viewModel.LoginSucceeded += OnLoginSucceeded;
    }

    /// <summary>
    /// PasswordBox không hỗ trợ Two-Way Binding trực tiếp trong Uno Platform
    /// → Cập nhật thủ công vào ViewModel khi password thay đổi
    /// </summary>
    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox pb)
        {
            _viewModel.Password = pb.Password;
        }
    }

    /// <summary>
    /// Điều hướng sau khi đăng nhập thành công dựa vào Role
    /// </summary>
    private void OnLoginSucceeded(string role)
    {
        _ = DispatcherQueue.TryEnqueue(() =>
        {
            if (role == "Admin")
            {
                // Admin → vào MainPage (đầy đủ menu)
                this.Frame?.Navigate(typeof(MainPage));
            }
            else
            {
                // Sale/Nhân viên → vào PosPage độc lập
                this.Frame?.Navigate(typeof(PosPage));
            }
        });
    }
}
