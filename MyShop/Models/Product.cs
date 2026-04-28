using CommunityToolkit.Mvvm.ComponentModel;
using System.Linq;

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

    // UI/Display helpers - computed properties
    public bool HasCustomization => Category?.Name != "Pastries";
    public string DisplayPrice => HasCustomization ? $"from {Price:C}" : $"{Price:C}";
    public string? PrimaryImageUrl =>
        ProductImages
            .OrderBy(pi => pi.SortOrder)
            .FirstOrDefault(pi => pi.IsPrimary)?.ImageUrl
        ?? ProductImages
            .OrderBy(pi => pi.SortOrder)
            .FirstOrDefault()?.ImageUrl
        ?? ImagePath;

    public IReadOnlyList<string> SecondaryImageUrls =>
        ProductImages
            .Where(pi => !pi.IsPrimary)
            .OrderBy(pi => pi.SortOrder)
            .Select(pi => pi.ImageUrl)
            .Take(2)
            .ToList();

    public bool HasPrimaryImage => !string.IsNullOrWhiteSpace(PrimaryImageUrl);
    public bool HasSecondaryImages => SecondaryImageUrls.Count > 0;

    // Navigation properties
    public Category? Category { get; set; }
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
}