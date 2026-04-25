using MyShop.Data;
using MyShop.Models;
using Microsoft.EntityFrameworkCore;

namespace MyShop.Services;

// ===== INTERFACE =====
public interface IAuthService
{
    /// <summary>
    /// Xác thực đăng nhập.
    /// Trả về User nếu đúng username + password, null nếu sai.
    /// </summary>
    Task<User?> LoginAsync(string username, string password);
}

// ===== IMPLEMENTATION =====
public class AuthService : IAuthService
{
    private readonly AppDbContextFactory _contextFactory;

    public AuthService(AppDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    /// <summary>
    /// Tìm user theo Username và so sánh PasswordHash.
    /// Lưu ý: hiện tại so sánh plain-text. Khi deploy thực tế
    /// nên dùng BCrypt.Verify() hoặc hàm băm tương đương.
    /// </summary>
    public async Task<User?> LoginAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return null;

        using var context = _contextFactory.CreateDbContext();

        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username.Trim() && u.IsActive);

        if (user == null) return null;

        // Xác thực mật khẩu Hash bằng BCrypt
        try
        {
            if (BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return user;
            }
        }
        catch (Exception)
        {
            // Nếu dữ liệu trong DB không phải chuỗi Hash hợp lệ, từ chối đăng nhập
            return null;
        }

        return null;
    }
}
