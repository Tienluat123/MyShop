namespace MyShop.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? IconGlyph { get; set; }
    
    // Navigation properties
    public ICollection<Product> Products { get; set; } = new List<Product>();
}