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
    private string? imagePath; // Đường dẫn ảnh

    // UI/Display helpers - computed properties
    public bool HasCustomization => Category?.Name != "Pastries";
    public string DisplayPrice =>
    HasCustomization
        ? $"from {Price.ToString("C", new CultureInfo("vi-VN"))}"
        : Price.ToString("C", new CultureInfo("vi-VN"));

    // Navigation properties
    public Category? Category { get; set; }
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
