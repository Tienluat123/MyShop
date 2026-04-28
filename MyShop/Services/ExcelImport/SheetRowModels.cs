namespace MyShop.Services.ExcelImport;

internal sealed class CategoryImportRow
{
    public required int RowNumber { get; init; }
    public required string Name { get; init; }
    public bool? HasCustomization { get; init; }
    public string? IconKey { get; init; }
    public string? Description { get; init; }
}

internal sealed class ProductImportRow
{
    public required int RowNumber { get; init; }
    public required string Name { get; init; }
    public required decimal Price { get; init; }
    public required string CategoryName { get; init; }
    public string? ImagePath { get; init; }
}
