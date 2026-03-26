namespace MyShop.Models;

public class Promotion
{
    public int Id { get; set; }
    public string PromoCode { get; set; } = string.Empty; // Mã (VD: GIAM30) - UNIQUE
    public string PromoType { get; set; } = "Percent"; // Percent / FixedAmount
    public decimal DiscountValue { get; set; } // Giá trị giảm (30 hoặc 50000)
    public decimal MinOrderAmount { get; set; } // Đơn tối thiểu áp dụng
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string TargetAudience { get; set; } = "All"; // All / VIP
    public bool IsActive { get; set; } = true; // Bật/Tắt mã
    
    // Navigation properties
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}