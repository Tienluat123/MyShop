using CommunityToolkit.Mvvm.ComponentModel;

namespace MyShop.Models;

public partial class Category : ObservableObject
{
    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string? iconGlyph;

    [ObservableProperty]
    private string? description;

    // Navigation properties
    public ICollection<Product> Products { get; set; } = new List<Product>();
}