namespace MyShop.Models;

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int UserId { get; set; } // Nhân viên xử lý
    public int? PromotionId { get; set; } // Mã KM áp dụng (Nếu có)
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public decimal SubTotal { get; set; } // Tổng tiền hàng
    public decimal DiscountAmount { get; set; } // Tiền được giảm
    public decimal TotalAmount { get; set; } // Tổng tiền phải thu
    public string Status { get; set; } = "New"; // New / Paid / Cancelled
    
    // Navigation properties
    public Customer? Customer { get; set; }
    public User? User { get; set; }
    public Promotion? Promotion { get; set; }
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}