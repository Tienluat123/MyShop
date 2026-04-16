using CommunityToolkit.Mvvm.ComponentModel; // Cung cấp ObservableObject và ObservableProperty
using System.Collections.Generic; // Cung cấp List
using System.Collections.ObjectModel; // Cung cấp ObservableCollection
using MyShop.Data; // Cung cấp các lớp dữ liệu như Order, WorkSchedule,


namespace MyShop.Models;

public partial class User : ObservableObject
{
    [ObservableProperty] private int id;
    [ObservableProperty] private string username = string.Empty; // login account (email/username)
    [ObservableProperty] private string phone = string.Empty; // phone number (09xx.xxx.xxx)

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullName))] // Khi FirstName hoặc LastName thay đổi, cũng thông báo rằng FullName đã thay đổi
    private string firstName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullName))] // Khi FirstName hoặc LastName thay đổi, cũng thông báo rằng FullName đã thay đổi
    private string lastName = string.Empty;
    [ObservableProperty] private string passwordHash = string.Empty;

    [ObservableProperty] private decimal hourlyRate; // Lương tính theo giờ

    [ObservableProperty] private string role = "2"; // Staff(2) / Admin(1) / SuperAdmin(0)

    [ObservableProperty] private bool isActive = true; // Trạng thái hoạt động, Khóa = false

    [ObservableProperty] private bool isEditing = false; // Trang thái chỉnh sửa (dùng cho UI)

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PasswordIcon))] // Tự động đổi Icon khi bấm
    private bool isPasswordVisible = false; // Show/hide password

    public string FullName => $"{FirstName} {LastName}".Trim();

    public string PasswordIcon => IsPasswordVisible ? "\uE734" : "\uE735";

    // Navigation properties
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<WorkSchedule> WorkSchedules { get; set; } = new List<WorkSchedule>();
    public ICollection<UserSetting> UserSettings { get; set; } = new List<UserSetting>();
}