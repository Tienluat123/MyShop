namespace MyShop.Models;

public class OrderDetail
{
    public int Id { get; set; }
    public int OrderId { get; set; } // Thuộc hóa đơn nào
    public int ProductId { get; set; } // Mua sản phẩm gì
    public int Quantity { get; set; } // Số lượng
    public decimal UnitPrice { get; set; } // Giá tại thời điểm mua
    
    // Navigation properties
    public Order? Order { get; set; }
    public Product? Product { get; set; }
}