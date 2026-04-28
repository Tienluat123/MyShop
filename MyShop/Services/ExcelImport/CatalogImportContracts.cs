namespace MyShop.Services.ExcelImport;

public interface IExcelCatalogImportService
{
    Task<CatalogImportResult> ImportAsync(string filePath, CancellationToken cancellationToken = default);
}

public sealed class CatalogImportResult
{
    public int CategoriesInserted { get; set; }
    public int CategoriesUpdated { get; set; }
    public int ProductsInserted { get; set; }
    public int ProductsUpdated { get; set; }
    public List<string> InsertedProductNames { get; } = new();

    public List<ImportError> Errors { get; } = new();
    public List<string> Warnings { get; } = new();

    public bool Success => Errors.Count == 0;

    public string Summary
    {
        get
        {
            if (!Success)
            {
                return $"Import failed with {Errors.Count} error(s).";
            }

            return $"Import completed. Categories: +{CategoriesInserted} / ~{CategoriesUpdated}; Products: +{ProductsInserted} / ~{ProductsUpdated}.";
        }
    }
}

public sealed class ImportError
{
    public required string Sheet { get; init; }
    public required int Row { get; init; }
    public required string Column { get; init; }
    public required string Message { get; init; }

    public override string ToString()
    {
        return $"{Sheet} row {Row}, column {Column}: {Message}";
    }
}
