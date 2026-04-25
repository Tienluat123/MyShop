using CommunityToolkit.Mvvm.ComponentModel;

namespace MyShop.Models;

public partial class User : ObservableObject
{
    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string passwordHash = string.Empty;

    [ObservableProperty]
    private string fullName = string.Empty;

    [ObservableProperty]
    private decimal hourlyRate; // Lương tính theo giờ

    [ObservableProperty]
    private string role = "2"; // Staff(2) / Admin(1) / SuperAdmin(0)

    [ObservableProperty]
    private bool isActive = true; // Trạng thái hoạt động, Khóa = false

    // Navigation properties
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<WorkSchedule> WorkSchedules { get; set; } = new List<WorkSchedule>();
    public ICollection<UserSetting> UserSettings { get; set; } = new List<UserSetting>();
}