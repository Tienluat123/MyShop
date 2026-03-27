using CommunityToolkit.Mvvm.ComponentModel;

namespace MyShop.Models;

public partial class Promotion : ObservableObject
{
    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private string promoCode = string.Empty; // Mã (VD: GIAM30) - UNIQUE

    [ObservableProperty]
    private string promoType = "Percent"; // Percent / FixedAmount

    [ObservableProperty]
    private decimal discountValue; // Giá trị giảm (30 hoặc 50000)

    [ObservableProperty]
    private decimal minOrderAmount; // Đơn tối thiểu áp dụng

    [ObservableProperty]
    private DateTime startDate;

    [ObservableProperty]
    private DateTime endDate;

    [ObservableProperty]
    private string targetAudience = "All"; // All / VIP

    [ObservableProperty]
    private bool isActive = true; // Bật/Tắt mã

    // Navigation properties
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}