using CommunityToolkit.Mvvm.ComponentModel;

namespace MyShop.Models;

public partial class ProductImage : ObservableObject
{
    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private int productId;

    [ObservableProperty]
    private string imageUrl = string.Empty;

    [ObservableProperty]
    private int sortOrder;

    [ObservableProperty]
    private bool isPrimary;

    [ObservableProperty]
    private DateTime createdAt;

    // Navigation properties
    public Product? Product { get; set; }
}