namespace MyShop.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; } // Lương tính theo giờ
    public string Role { get; set; } = "2"; // Staff(2) / Admin(1) / SuperAdmin(0)
    public bool IsActive { get; set; } = true; // Trạng thái hoạt động, Khóa = false
    
    // Navigation properties
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<WorkSchedule> WorkSchedules { get; set; } = new List<WorkSchedule>();
    public ICollection<UserSetting> UserSettings { get; set; } = new List<UserSetting>();
}