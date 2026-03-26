namespace MyShop.Models;

public class UserSetting
{
    public int Id { get; set; }
    public int UserId { get; set; } // Của nhân viên nào
    public string SettingKey { get; set; } = string.Empty; // Khóa (VD: ItemsPerPage, DefaultScreen)
    public string SettingValue { get; set; } = string.Empty; // Giá trị
    
    // Navigation properties
    public User? User { get; set; }
}
