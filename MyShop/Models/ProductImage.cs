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
    private int sortOrder = 1;

    [ObservableProperty]
    private bool isPrimary;

    [ObservableProperty]
    private DateTime createdAt = DateTime.UtcNow;

    public Product? Product { get; set; }
}
