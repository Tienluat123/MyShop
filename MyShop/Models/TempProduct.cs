using CommunityToolkit.Mvvm.ComponentModel;

namespace MyShop.Models;

public partial class TempProduct : ObservableObject
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public double Price { get; set; }
    public string Category { get; set; } = "";
    public string ImageUrl { get; set; } = "";

    public bool HasCustomization => Category != "Pastries";

    public string DisplayPrice =>
        HasCustomization ? $"from ${Price:F2}" : $"${Price:F2}";
}
