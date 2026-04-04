using CommunityToolkit.Mvvm.ComponentModel;

namespace MyShop.Models;

public partial class Order : ObservableObject
{
    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private int customerId;

    [ObservableProperty]
    private int userId; // Nhân viên xử lý

    [ObservableProperty]
    private int? promotionId; // Mã KM áp dụng (Nếu có)

    [ObservableProperty]
    private DateTime orderDate = DateTime.UtcNow;

    [ObservableProperty]
    private decimal subTotal; // Tổng tiền hàng

    [ObservableProperty]
    private decimal discountAmount; // Tiền được giảm

    [ObservableProperty]
    private decimal totalAmount; // Tổng tiền phải thu

    [ObservableProperty]
    private string paymentStatus = "Draft"; // Draft / PendingPayment / Paid / Cancelled / Refunded

    [ObservableProperty]
    private string fulfillmentStatus = "None"; // None / Preparing / Ready / Completed

    // Navigation properties
    public Customer? Customer { get; set; }
    public User? User { get; set; }
    public Promotion? Promotion { get; set; }
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
