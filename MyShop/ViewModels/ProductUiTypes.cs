namespace MyShop.ViewModels;

public sealed class ProductListEntry
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }

    public string SellingPriceText => $"{SellingPrice:N0} d";
    public string CostPriceText => $"{CostPrice:N0} d";
}

public sealed class ProductInputData
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CostPrice { get; set; } = string.Empty;
    public string SellingPrice { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}
