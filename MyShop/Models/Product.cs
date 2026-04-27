using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MyShop.Models;

public partial class Product : ObservableObject
{
    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private int categoryId; // Thuộc loại nào

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private decimal price; // Giá bán

    [ObservableProperty]
    private decimal baseCostPrice; // Giá vốn

    [ObservableProperty]
    private string? imagePath; // Đường dẫn ảnh

    // Navigation properties
    public Category? Category { get; set; }
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
