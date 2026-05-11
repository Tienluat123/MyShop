using System.Globalization;
using Microsoft.EntityFrameworkCore;
using MyShop.Data;
using MyShop.Models;

namespace MyShop.Services.ExcelImport;

public sealed class ExcelCatalogImportService : IExcelCatalogImportService
{
    private const string CategoriesSheet = "Categories";
    private const string ProductsSheet = "Products";

    private static readonly string[][] RequiredCategoryHeaderGroups =
    [
        ["Name"]
    ];

    private static readonly string[][] RequiredProductHeaderGroups =
    [
        ["Name"],
        ["Price"],
        ["Category", "CategoryCode"]
    ];

    private readonly AppDbContextFactory _contextFactory;

    public ExcelCatalogImportService(AppDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
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

            ValidateSheetAndHeaders(reader, CategoriesSheet, RequiredCategoryHeaderGroups, result);
            ValidateSheetAndHeaders(reader, ProductsSheet, RequiredProductHeaderGroups, result);

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

            await using var context = _contextFactory.CreateDbContext(Array.Empty<string>());
            var executionStrategy = context.Database.CreateExecutionStrategy();

            await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

                var categoriesByName = await UpsertCategoriesAsync(context, categoryRows, result, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
                await UpsertProductsAsync(context, productRows, categoriesByName, result, cancellationToken);

                await context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            });

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
        IEnumerable<string[]> requiredHeaderGroups,
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
        foreach (var requiredHeaderGroup in requiredHeaderGroups)
        {
            if (!requiredHeaderGroup.Any(header => headers.Contains(header, StringComparer.OrdinalIgnoreCase)))
            {
                result.Errors.Add(new ImportError
                {
                    Sheet = sheetName,
                    Row = 1,
                    Column = string.Join(" / ", requiredHeaderGroup),
                    Message = $"Missing required header '{string.Join("' or '", requiredHeaderGroup)}'."
                });
            }
        }
    }

    private static List<CategoryImportRow> ParseCategoryRows(
        List<ExcelRowData> rows,
        CatalogImportResult result)
    {
        var parsed = new List<CategoryImportRow>();
        var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            var name = row.GetValue("Name");
            if (string.IsNullOrWhiteSpace(name))
            {
                result.Errors.Add(NewError(CategoriesSheet, row.RowNumber, "Name", "Name is required."));
                continue;
            }

            if (!seenNames.Add(name))
            {
                result.Errors.Add(NewError(CategoriesSheet, row.RowNumber, "Name", "Name is duplicated in file."));
                continue;
            }

            var hasCustomizationRaw = row.GetValue("HasCustomization");
            bool? hasCustomization = null;

            if (!string.IsNullOrWhiteSpace(hasCustomizationRaw))
            {
                if (!TryParseBool(hasCustomizationRaw, out var parsedHasCustomization))
                {
                    result.Errors.Add(NewError(CategoriesSheet, row.RowNumber, "HasCustomization", "Use TRUE/FALSE, 1/0, yes/no."));
                    continue;
                }

                hasCustomization = parsedHasCustomization;
            }

            parsed.Add(new CategoryImportRow
            {
                RowNumber = row.RowNumber,
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
        var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            var name = row.GetValue("Name");
            var priceRaw = row.GetValue("Price");
            var categoryName = GetFirstValue(row, "Category", "CategoryCode");

            if (string.IsNullOrWhiteSpace(name))
            {
                result.Errors.Add(NewError(ProductsSheet, row.RowNumber, "Name", "Name is required."));
                continue;
            }

            if (string.IsNullOrWhiteSpace(categoryName))
            {
                result.Errors.Add(NewError(ProductsSheet, row.RowNumber, "Category", "Category is required."));
                continue;
            }

            var productKey = BuildProductKey(name, categoryName);
            if (!seenKeys.Add(productKey))
            {
                result.Errors.Add(NewError(ProductsSheet, row.RowNumber, "Name", "Product name and category are duplicated in file."));
                continue;
            }

            if (!decimal.TryParse(priceRaw, NumberStyles.Number, CultureInfo.InvariantCulture, out var price)
                && !decimal.TryParse(priceRaw, NumberStyles.Number, CultureInfo.CurrentCulture, out price))
            {
                result.Errors.Add(NewError(ProductsSheet, row.RowNumber, "Price", "Price is invalid."));
                continue;
            }

            if (price < 0)
            {
                result.Errors.Add(NewError(ProductsSheet, row.RowNumber, "Price", "Price must be >= 0."));
                continue;
            }

            parsed.Add(new ProductImportRow
            {
                RowNumber = row.RowNumber,
                Name = name,
                CategoryName = categoryName,
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
        var categoryNames = categories
            .Select(c => c.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var product in products)
        {
            if (!categoryNames.Contains(product.CategoryName))
            {
                result.Errors.Add(NewError(
                    ProductsSheet,
                    product.RowNumber,
                    "Category",
                    $"Category '{product.CategoryName}' was not found in Categories sheet."));
            }
        }
    }

    private static async Task<Dictionary<string, Category>> UpsertCategoriesAsync(
        AppDbContext context,
        List<CategoryImportRow> rows,
        CatalogImportResult result,
        CancellationToken cancellationToken)
    {
        var categoryByName = new Dictionary<string, Category>(StringComparer.OrdinalIgnoreCase);
        var existingCategories = await context.Categories.ToListAsync(cancellationToken);
        var existingByName = existingCategories.ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (existingByName.TryGetValue(row.Name, out var existing))
            {
                var hasChanges = false;

                if (row.HasCustomization.HasValue && existing.HasCustomization != row.HasCustomization.Value)
                {
                    existing.HasCustomization = row.HasCustomization.Value;
                    hasChanges = true;
                }

                if (row.IconKey != null && !string.Equals(existing.IconKey, row.IconKey, StringComparison.Ordinal))
                {
                    existing.IconKey = row.IconKey;
                    hasChanges = true;
                }

                if (row.Description != null && !string.Equals(existing.Description, row.Description, StringComparison.Ordinal))
                {
                    existing.Description = row.Description;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    result.CategoriesUpdated++;
                }

                categoryByName[row.Name] = existing;
                continue;
            }

            var created = new Category
            {
                Name = row.Name,
                HasCustomization = row.HasCustomization ?? false,
                IconKey = row.IconKey,
                Description = row.Description
            };

            context.Categories.Add(created);
            result.CategoriesInserted++;
            categoryByName[row.Name] = created;
        }

        return categoryByName;
    }

    private static async Task UpsertProductsAsync(
        AppDbContext context,
        List<ProductImportRow> rows,
        Dictionary<string, Category> categoriesByName,
        CatalogImportResult result,
        CancellationToken cancellationToken)
    {
        var existingProducts = await context.Products.ToListAsync(cancellationToken);
        var existingByNameAndCategory = existingProducts.ToDictionary(
            p => BuildProductKey(p.Name, p.CategoryId),
            StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!categoriesByName.TryGetValue(row.CategoryName, out var category))
            {
                continue;
            }

            var productKey = BuildProductKey(row.Name, category.Id);

            if (existingByNameAndCategory.TryGetValue(productKey, out var existing))
            {
                var hasChanges = existing.Price != row.Price || existing.CategoryId != category.Id;

                if (row.ImagePath != null && !string.Equals(existing.ImagePath, row.ImagePath, StringComparison.Ordinal))
                {
                    existing.ImagePath = row.ImagePath;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    existing.Price = row.Price;
                    existing.CategoryId = category.Id;
                    result.ProductsUpdated++;
                }

                continue;
            }

            var created = new Product
            {
                Name = row.Name,
                CategoryId = category.Id,
                Price = row.Price,
                ImagePath = row.ImagePath
            };

            context.Products.Add(created);
            result.ProductsInserted++;
            result.InsertedProductNames.Add(row.Name);
            existingByNameAndCategory[productKey] = created;
        }
    }

    private static string BuildProductKey(string name, int categoryId)
    {
        return $"{name}|{categoryId}";
    }

    private static string BuildProductKey(string name, string categoryName)
    {
        return $"{name}|{categoryName}";
    }

    private static string GetFirstValue(ExcelRowData row, params string[] headerNames)
    {
        foreach (var headerName in headerNames)
        {
            var value = row.GetValue(headerName);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return string.Empty;
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