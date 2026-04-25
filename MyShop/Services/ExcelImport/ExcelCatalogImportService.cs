using System.Globalization;
using MyShop.Data;
using MyShop.Models;

namespace MyShop.Services.ExcelImport;

public sealed class ExcelCatalogImportService : IExcelCatalogImportService
{
    private const string CategoriesSheet = "Categories";
    private const string ProductsSheet = "Products";

    private static readonly string[] RequiredCategoryHeaders =
    [
        "CategoryCode",
        "Name",
        "HasCustomization"
    ];

    private static readonly string[] RequiredProductHeaders =
    [
        "ProductCode",
        "Name",
        "Price",
        "CategoryCode"
    ];

    private readonly IRepository<Category> _categoryRepository;
    private readonly IRepository<Product> _productRepository;

    public ExcelCatalogImportService(
        IRepository<Category> categoryRepository,
        IRepository<Product> productRepository)
    {
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
    }

    public async Task<CatalogImportResult> ImportAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var result = new CatalogImportResult();

        if (string.IsNullOrWhiteSpace(filePath))
        {
            result.Errors.Add(new ImportError
            {
                Sheet = "Workbook",
                Row = 0,
                Column = "FilePath",
                Message = "File path is required."
            });
            return result;
        }

        if (!File.Exists(filePath))
        {
            result.Errors.Add(new ImportError
            {
                Sheet = "Workbook",
                Row = 0,
                Column = "FilePath",
                Message = "File does not exist."
            });
            return result;
        }

        if (!filePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            result.Errors.Add(new ImportError
            {
                Sheet = "Workbook",
                Row = 0,
                Column = "FilePath",
                Message = "Only .xlsx files are supported in phase 1."
            });
            return result;
        }

        try
        {
            using var reader = new ExcelWorkbookReader(filePath);

            ValidateSheetAndHeaders(reader, CategoriesSheet, RequiredCategoryHeaders, result);
            ValidateSheetAndHeaders(reader, ProductsSheet, RequiredProductHeaders, result);

            if (!result.Success)
            {
                return result;
            }

            var categoryRows = ParseCategoryRows(reader.ReadRows(CategoriesSheet), result);
            var productRows = ParseProductRows(reader.ReadRows(ProductsSheet), result);

            ValidateCrossSheetReferences(categoryRows, productRows, result);

            if (!result.Success)
            {
                return result;
            }

            cancellationToken.ThrowIfCancellationRequested();

            // TEMP: DB write is disabled. Keep only parse/validate for now.
            // var categoryCodeToId = await UpsertCategories(categoryRows, result, cancellationToken);
            // await UpsertProducts(productRows, categoryCodeToId, result, cancellationToken);
            result.Warnings.Add("Database upsert is disabled (dry-run mode).");

            return result;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            result.Errors.Add(new ImportError
            {
                Sheet = "Workbook",
                Row = 0,
                Column = "General",
                Message = ex.Message
            });
            return result;
        }
    }

    private static void ValidateSheetAndHeaders(
        ExcelWorkbookReader reader,
        string sheetName,
        IEnumerable<string> requiredHeaders,
        CatalogImportResult result)
    {
        if (!reader.HasSheet(sheetName))
        {
            result.Errors.Add(new ImportError
            {
                Sheet = sheetName,
                Row = 0,
                Column = "Sheet",
                Message = $"Required sheet '{sheetName}' is missing."
            });
            return;
        }

        var headers = reader.GetHeaders(sheetName);
        foreach (var requiredHeader in requiredHeaders)
        {
            if (!headers.Contains(requiredHeader, StringComparer.OrdinalIgnoreCase))
            {
                result.Errors.Add(new ImportError
                {
                    Sheet = sheetName,
                    Row = 1,
                    Column = requiredHeader,
                    Message = $"Missing required header '{requiredHeader}'."
                });
            }
        }
    }

    private static List<CategoryImportRow> ParseCategoryRows(
        List<ExcelRowData> rows,
        CatalogImportResult result)
    {
        var parsed = new List<CategoryImportRow>();
        var seenCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            var code = row.GetValue("CategoryCode");
            var name = row.GetValue("Name");
            var hasCustomizationRaw = row.GetValue("HasCustomization");

            if (string.IsNullOrWhiteSpace(code))
            {
                result.Errors.Add(NewError(CategoriesSheet, row.RowNumber, "CategoryCode", "CategoryCode is required."));
                continue;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                result.Errors.Add(NewError(CategoriesSheet, row.RowNumber, "Name", "Name is required."));
                continue;
            }

            if (!seenCodes.Add(code))
            {
                result.Errors.Add(NewError(CategoriesSheet, row.RowNumber, "CategoryCode", "CategoryCode is duplicated in file."));
                continue;
            }

            if (!TryParseBool(hasCustomizationRaw, out var hasCustomization))
            {
                result.Errors.Add(NewError(CategoriesSheet, row.RowNumber, "HasCustomization", "Use TRUE/FALSE or 1/0."));
                continue;
            }

            parsed.Add(new CategoryImportRow
            {
                RowNumber = row.RowNumber,
                CategoryCode = code,
                Name = name,
                HasCustomization = hasCustomization,
                IconKey = NullIfEmpty(row.GetValue("IconKey")),
                Description = NullIfEmpty(row.GetValue("Description"))
            });
        }

        return parsed;
    }

    private static List<ProductImportRow> ParseProductRows(
        List<ExcelRowData> rows,
        CatalogImportResult result)
    {
        var parsed = new List<ProductImportRow>();
        var seenCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            var productCode = row.GetValue("ProductCode");
            var name = row.GetValue("Name");
            var categoryCode = row.GetValue("CategoryCode");
            var priceRaw = row.GetValue("Price");

            if (string.IsNullOrWhiteSpace(productCode))
            {
                result.Errors.Add(NewError(ProductsSheet, row.RowNumber, "ProductCode", "ProductCode is required."));
                continue;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                result.Errors.Add(NewError(ProductsSheet, row.RowNumber, "Name", "Name is required."));
                continue;
            }

            if (string.IsNullOrWhiteSpace(categoryCode))
            {
                result.Errors.Add(NewError(ProductsSheet, row.RowNumber, "CategoryCode", "CategoryCode is required."));
                continue;
            }

            if (!seenCodes.Add(productCode))
            {
                result.Errors.Add(NewError(ProductsSheet, row.RowNumber, "ProductCode", "ProductCode is duplicated in file."));
                continue;
            }

            if (!decimal.TryParse(priceRaw, NumberStyles.Number, CultureInfo.InvariantCulture, out var price))
            {
                if (!decimal.TryParse(priceRaw, NumberStyles.Number, CultureInfo.CurrentCulture, out price))
                {
                    result.Errors.Add(NewError(ProductsSheet, row.RowNumber, "Price", "Price is invalid."));
                    continue;
                }
            }

            if (price < 0)
            {
                result.Errors.Add(NewError(ProductsSheet, row.RowNumber, "Price", "Price must be >= 0."));
                continue;
            }

            parsed.Add(new ProductImportRow
            {
                RowNumber = row.RowNumber,
                ProductCode = productCode,
                Name = name,
                CategoryCode = categoryCode,
                Price = price,
                ImagePath = NullIfEmpty(row.GetValue("ImagePath"))
            });
        }

        return parsed;
    }

    private static void ValidateCrossSheetReferences(
        List<CategoryImportRow> categories,
        List<ProductImportRow> products,
        CatalogImportResult result)
    {
        var categoryCodes = categories
            .Select(c => c.CategoryCode)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var product in products)
        {
            if (!categoryCodes.Contains(product.CategoryCode))
            {
                result.Errors.Add(NewError(
                    ProductsSheet,
                    product.RowNumber,
                    "CategoryCode",
                    $"CategoryCode '{product.CategoryCode}' was not found in Categories sheet."));
            }
        }
    }

    private async Task<Dictionary<string, int>> UpsertCategories(
        List<CategoryImportRow> rows,
        CatalogImportResult result,
        CancellationToken cancellationToken)
    {
        var categoryCodeToId = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var existingCategories = await _categoryRepository.GetAsync();
        var existingByName = existingCategories.ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (existingByName.TryGetValue(row.Name, out var existing))
            {
                var hasChanges = existing.HasCustomization != row.HasCustomization
                    || !string.Equals(existing.IconKey, row.IconKey, StringComparison.Ordinal)
                    || !string.Equals(existing.Description, row.Description, StringComparison.Ordinal);

                if (hasChanges)
                {
                    existing.HasCustomization = row.HasCustomization;
                    existing.IconKey = row.IconKey;
                    existing.Description = row.Description;
                    await _categoryRepository.UpdateAsync(existing);
                    result.CategoriesUpdated++;
                }

                categoryCodeToId[row.CategoryCode] = existing.Id;
                continue;
            }

            var created = await _categoryRepository.AddAsync(new Category
            {
                Name = row.Name,
                HasCustomization = row.HasCustomization,
                IconKey = row.IconKey,
                Description = row.Description
            });

            result.CategoriesInserted++;
            categoryCodeToId[row.CategoryCode] = created.Id;
            existingByName[row.Name] = created;
        }

        return categoryCodeToId;
    }

    private async Task UpsertProducts(
        List<ProductImportRow> rows,
        Dictionary<string, int> categoryCodeToId,
        CatalogImportResult result,
        CancellationToken cancellationToken)
    {
        var existingProducts = await _productRepository.GetAsync();
        var existingByNameAndCategory = existingProducts.ToDictionary(
            p => BuildProductKey(p.Name, p.CategoryId),
            StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var categoryId = categoryCodeToId[row.CategoryCode];
            var productKey = BuildProductKey(row.Name, categoryId);

            if (existingByNameAndCategory.TryGetValue(productKey, out var existing))
            {
                var hasChanges = existing.Price != row.Price
                    || !string.Equals(existing.ImagePath, row.ImagePath, StringComparison.Ordinal)
                    || existing.CategoryId != categoryId;

                if (hasChanges)
                {
                    existing.Price = row.Price;
                    existing.ImagePath = row.ImagePath;
                    existing.CategoryId = categoryId;
                    await _productRepository.UpdateAsync(existing);
                    result.ProductsUpdated++;
                }

                continue;
            }

            var created = await _productRepository.AddAsync(new Product
            {
                Name = row.Name,
                CategoryId = categoryId,
                Price = row.Price,
                ImagePath = row.ImagePath
            });

            result.ProductsInserted++;
            existingByNameAndCategory[productKey] = created;
        }
    }

    private static string BuildProductKey(string name, int categoryId)
    {
        return $"{name}|{categoryId}";
    }

    private static bool TryParseBool(string raw, out bool value)
    {
        if (bool.TryParse(raw, out value))
        {
            return true;
        }

        if (string.Equals(raw, "1", StringComparison.OrdinalIgnoreCase)
            || string.Equals(raw, "yes", StringComparison.OrdinalIgnoreCase)
            || string.Equals(raw, "y", StringComparison.OrdinalIgnoreCase))
        {
            value = true;
            return true;
        }

        if (string.Equals(raw, "0", StringComparison.OrdinalIgnoreCase)
            || string.Equals(raw, "no", StringComparison.OrdinalIgnoreCase)
            || string.Equals(raw, "n", StringComparison.OrdinalIgnoreCase))
        {
            value = false;
            return true;
        }

        value = false;
        return false;
    }

    private static string? NullIfEmpty(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static ImportError NewError(string sheet, int row, string column, string message)
    {
        return new ImportError
        {
            Sheet = sheet,
            Row = row,
            Column = column,
            Message = message
        };
    }
}
