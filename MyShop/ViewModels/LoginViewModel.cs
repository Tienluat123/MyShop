using MyShop.Models;
using MyShop.Services;
using Microsoft.UI.Dispatching;

namespace MyShop.ViewModels;

/// <summary>
/// ViewModel cho màn hình Đăng nhập.
/// Kế thừa BaseViewModel để dùng sẵn IsLoading, ErrorMessage, SetProperty.
/// </summary>
public class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly ISessionService _sessionService;
    private readonly DispatcherQueue _dispatcher;

    // ===== PROPERTIES (bound to LoginPage.xaml) =====

    private string _username = string.Empty;
    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    private bool _isPasswordVisible;
    public bool IsPasswordVisible
    {
        get => _isPasswordVisible;
        set => SetProperty(ref _isPasswordVisible, value);
    }

    // ===== EVENT: thông báo cho View navigate sau khi login thành công =====
    // Tham số: "MainPage" hoặc "PosPage"
    public event Action<string>? LoginSucceeded;

    // ===== COMMAND =====
    // Dùng AsyncRelayCommand để await đúng LoginAsync (tránh fire-and-forget mất exception)
    public AsyncRelayCommand LoginCommand { get; }
    public RelayCommand TogglePasswordCommand { get; }

    // ===== CONSTRUCTOR =====
    public LoginViewModel(IAuthService authService, ISessionService sessionService)
    {
        _authService = authService;
        _sessionService = sessionService;
        // Lấy DispatcherQueue của UI thread để cập nhật ErrorMessage an toàn
        _dispatcher = DispatcherQueue.GetForCurrentThread();
        LoginCommand = new AsyncRelayCommand(LoginAsync);
        TogglePasswordCommand = new RelayCommand(() => IsPasswordVisible = !IsPasswordVisible);
    }

    // ===== HÀM HỖ TRỢ =====
    /// <summary>
    /// Set property trên UI thread an toàn dù gọi từ background thread
    /// </summary>
    private void SetOnUiThread(Action action)
    {
        if (_dispatcher.HasThreadAccess)
            action();
        else
            _dispatcher.TryEnqueue(() => action());
    }

    // ===== LOGIC ĐĂNG NHẬP =====
    private async Task LoginAsync()
    {
        // Validate input cơ bản
        if (string.IsNullOrWhiteSpace(Username))
        {
            SetOnUiThread(() => ErrorMessage = "Please enter your username.");
            _ = Task.Delay(3000).ContinueWith(_ => SetOnUiThread(() => { if (ErrorMessage == "Please enter your username.") ErrorMessage = string.Empty; }));
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            SetOnUiThread(() => ErrorMessage = "Please enter your password.");
            _ = Task.Delay(3000).ContinueWith(_ => SetOnUiThread(() => { if (ErrorMessage == "Please enter your password.") ErrorMessage = string.Empty; }));
            return;
        }

        SetOnUiThread(() => { IsLoading = true; ErrorMessage = string.Empty; });

        try
        {
            User? user = await _authService.LoginAsync(Username, Password);

            if (user is null)
            {
                SetOnUiThread(() => ErrorMessage = "Incorrect username or password.");
                // Tự động tắt thông báo sau 3 giây
                _ = Task.Delay(3000).ContinueWith(_ => SetOnUiThread(() => { if (ErrorMessage == "Incorrect username or password.") ErrorMessage = string.Empty; }));
                return;
            }

            // Lưu vào Session để các Page khác có thể lấy thông tin User
            _sessionService.CurrentUser = user;

            // Phân quyền điều hướng (Admin hoặc Sale) để View biết chuyển đi đâu
            string destination = user.Role == "1" ? "Admin" : "Sale";
            SetOnUiThread(() => LoginSucceeded?.Invoke(destination));
        }
        catch (Exception ex)
        {
            SetOnUiThread(() => ErrorMessage = $"Connection error: {ex.Message}");
            _ = Task.Delay(3000).ContinueWith(_ => SetOnUiThread(() => ErrorMessage = string.Empty));
        }
        finally
        {
            SetOnUiThread(() => IsLoading = false);
        }
    }
}
