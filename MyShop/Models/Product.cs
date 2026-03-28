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
    public string DisplayPrice => HasCustomization ? $"from {Price:C}" : $"{Price:C}";

    // Navigation properties
    public Category? Category { get; set; }
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}