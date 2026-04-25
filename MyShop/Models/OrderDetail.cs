using CommunityToolkit.Mvvm.ComponentModel;

namespace MyShop.Models;

public partial class OrderDetail : ObservableObject
{
    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private int orderId; // Thuộc hóa đơn nào

    [ObservableProperty]
    private int productId; // Mua sản phẩm gì

    [ObservableProperty]
    private int quantity; // Số lượng

    [ObservableProperty]
    private decimal unitPrice; // Giá tại thời điểm mua

    // Navigation properties
    public Order? Order { get; set; }
    public Product? Product { get; set; }
}