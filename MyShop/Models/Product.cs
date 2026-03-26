namespace MyShop.Models;

public class Product
{
    public int Id { get; set; }
    public int CategoryId { get; set; } // Thuộc loại nào
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; } // Giá bán
    public string? ImagePath { get; set; } // Đường dẫn ảnh

    // UI/Display helpers
    public bool HasCustomization => Category?.Name != "Pastries";
    public string DisplayPrice => HasCustomization ? $"from {Price:C}" : $"{Price:C}";

    // Navigation properties
    public Category? Category { get; set; }
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}