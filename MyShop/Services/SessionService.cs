using MyShop.Models;

namespace MyShop.Services;

/// <summary>
/// Service lưu trữ thông tin phiên làm việc hiện tại (User đang đăng nhập)
/// </summary>
public interface ISessionService
{
    User? CurrentUser { get; set; }
    void Logout();
}

public class SessionService : ISessionService
{
    public User? CurrentUser { get; set; }

    public void Logout()
    {
        CurrentUser = null;
    }
}
