using MyShop.Models;
using MyShop.Services;
using MyShop.Services.ExcelImport;
using MyShop.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace MyShop.ViewModels;

public partial class ProductManagementViewModel : ListManagementViewModelBase
{
    private readonly IProductService _productService;
    private readonly IAppConfigService _configService;
    private readonly IExcelCatalogImportService _excelCatalogImportService;
    private readonly INotificationService _notificationService;

    private const string CategoryAll = "All";
    private const string CategoryCoffee = "Coffee";
    private const string CategoryTea = "Tea";
    private const string CategorySmoothies = "Smoothies";
    private const string CategoryPastries = "Pastries";

    private readonly Action onAddProduct;
    private readonly Action<ProductListEntry> onEditProduct;
    public ObservableCollection<string> Categories { get; } = new();
    public ObservableCollection<ProductListEntry> DisplayProducts { get; } = new();
    public ObservableCollection<int> PageNumbers { get; } = new();

    [ObservableProperty]
    private string selectedCategory = CategoryAll;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private int pageSize = 10;

    [ObservableProperty]
    private int currentPage = 1;

    [ObservableProperty]
    private int totalItems;

    [ObservableProperty]
    private int totalPages = 1;

    [ObservableProperty]
    private string pagingSummary = "Showing 0-0 in 0 items";

    [ObservableProperty]
    private double? minPrice;

    [ObservableProperty]
    private double? maxPrice;

    [ObservableProperty]
    private string pageInfoDisplay = "Page 1 / 1";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SortInfo))]
    private string sortColumn = "Name";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SortInfo))]
    private bool sortAscending = true;

    [ObservableProperty]
    private ObservableCollection<SortCriteriaItem> sortCriteriaList = new();

    public ObservableCollection<string> AvailableSortColumns { get; } = new()
    {
        "Name", "Price", "Category"
    };

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool hasNoData;

    public ProductManagementViewModel(
        IProductService productService,
        IAppConfigService configService,
        IExcelCatalogImportService excelCatalogImportService,
        INotificationService notificationService,
        Action onAddProduct,
        Action<ProductListEntry> onEditProduct)
    {
        _productService = productService;
        _configService = configService;
        _excelCatalogImportService = excelCatalogImportService;
        _notificationService = notificationService;

        this.onAddProduct = onAddProduct;
        this.onEditProduct = onEditProduct;

        GetAllCategories();
        _ = LoadProducts();
    }

    [RelayCommand]
    private async Task LoadProducts()
    {
        try
        {
            ErrorMessage = string.Empty;
            
            // Xử lý PageSize từ cấu hình
            PageSize = await _configService.GetPageSizeAsync();

            // Handle sorting
            string sortProperty = "";
            if (SortCriteriaList.Count > 0)
            {
                var sortStrings = new List<string>();
                foreach (var item in SortCriteriaList)
                {
                    sortStrings.Add($"{item.ColumnName} {item.Direction}");
                }
                sortProperty = string.Join(", ", sortStrings);
            }
            else
            {
                var direction = SortAscending ? "ASC" : "DESC";
                sortProperty = $"{SortColumn} {direction}";
            }

            var request = new PagingRequest
            {
                PageNumber = CurrentPage,
                PageSize = PageSize,
                SearchText = SearchText?.Trim(),
                Filters = new Dictionary<string, string>
                {
                    { "Category", SelectedCategory },
                    { "MinPrice", MinPrice?.ToString() ?? "" },
                    { "MaxPrice", MaxPrice?.ToString() ?? "" }
                },
                SortBy = sortProperty
            };

            var result = await _productService.GetAll(request);

            TotalItems = result.Pagination.TotalItems;
            TotalPages = result.Pagination.TotalPages;

            if (CurrentPage > TotalPages)
                CurrentPage = TotalPages;

            PageNumbers.Clear();
            for (int i = 1; i <= TotalPages; i++)
                PageNumbers.Add(i);

            PageInfoDisplay = $"Page {CurrentPage} / {TotalPages}";

            DisplayProducts.Clear();
            foreach (var p in result.Items)
            {
                DisplayProducts.Add(new ProductListEntry
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = p.Category?.Name ?? "",
                    SellingPrice = p.Price,
                    PrimaryImageUrl = p.PrimaryImageUrl,
                    SecondaryImageUrls = p.SecondaryImageUrls
                });
            }

            HasNoData = DisplayProducts.Count == 0;
            var pageStart = TotalItems == 0 ? 0 : (CurrentPage - 1) * PageSize + 1;
            var pageEnd = pageStart + DisplayProducts.Count - 1;
            PagingSummary = HasNoData
                ? "No products found"
                : $"Showing {pageStart}-{pageEnd} in {TotalItems} items";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load products: {ex.Message}";
            HasNoData = true;
            DisplayProducts.Clear();
            PageNumbers.Clear();
            PagingSummary = "No products found";
        }
    }

    [RelayCommand]
    private async Task SearchByName(string keyword)
    {
        SearchText = keyword;
        CurrentPage = 1;
        await LoadProducts();
    }

    [RelayCommand]
    private async Task FilterByCategory(string category)
    {
        SelectedCategory = category;
        CurrentPage = 1;
        await LoadProducts();
    }

    [RelayCommand]
    private async Task FilterByPriceRange()
    {
        CurrentPage = 1;
        await LoadProducts();
    }

    [RelayCommand]
    public void AddSortCriteria()
    {
        SortCriteriaList.Add(new SortCriteriaItem(AvailableSortColumns));
    }

    [RelayCommand]
    public void RemoveSortCriteria(SortCriteriaItem item)
    {
        if (item != null && SortCriteriaList.Contains(item))
        {
            SortCriteriaList.Remove(item);
        }
    }

    [RelayCommand]
    public async Task ApplyMultiSort()
    {
        CurrentPage = 1;
        await LoadProducts();
    }

    [RelayCommand]
    public async Task ClearMultiSort()
    {
        SortCriteriaList.Clear();
        CurrentPage = 1;
        await LoadProducts();
    }

    public string SortInfo => $"{SortColumn}|{SortAscending}";

    [RelayCommand]
    private async Task SortByColumn(string columnName)
    {
        if (string.IsNullOrWhiteSpace(columnName)) return;

        if (SortColumn == columnName)
        {
            SortAscending = !SortAscending;
        }
        else
        {
            SortColumn = columnName;
            SortAscending = true;
        }

        CurrentPage = 1;
        await LoadProducts();
    }

    [RelayCommand]
    private async Task LoadPage(int pageNumber)
    {
        if (pageNumber < 1 || pageNumber > TotalPages)
            return;

        CurrentPage = pageNumber;
        await LoadProducts();
    }

    [RelayCommand]
    private async Task PreviousPage()
    {
        if (CurrentPage <= 1) return;

        CurrentPage--;
        await LoadProducts();
    }

    [RelayCommand]
    private async Task NextPage()
    {
        if (CurrentPage >= TotalPages) return;

        CurrentPage++;
        await LoadProducts();
    }

    [RelayCommand]
    private void AddProduct()
    {
        onAddProduct();
    }

    [RelayCommand]
    private async Task ImportFromExcel(string filePath)
    {
        try
        {
            ErrorMessage = string.Empty;

            var importResult = await _excelCatalogImportService.ImportAsync(filePath);
            if (!importResult.Success)
            {
                var topErrors = importResult.Errors
                    .Take(5)
                    .Select(e => e.ToString());

                ErrorMessage = string.Join(Environment.NewLine, topErrors);
                return;
            }

            ErrorMessage = importResult.Summary;
            var successMessage = BuildImportSuccessMessage(importResult);
            _notificationService.ShowSuccess(successMessage, 3500);
            CurrentPage = 1;
            await LoadProducts();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to import from Excel: {ex.Message}";
        }
    }

    [RelayCommand]
    private void EditProduct(ProductListEntry product)
    {
        onEditProduct(product);
    }

    [RelayCommand]
    private async Task DeleteProduct(ProductListEntry product)
    {
        if (product == null) return;

        try
        {
            ErrorMessage = string.Empty;
            await _productService.Delete(product.Id);
            await LoadProducts();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to delete product: {ex.Message}";
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        CurrentPage = 1;
        _ = LoadProducts();
    }

    partial void OnSelectedCategoryChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            selectedCategory = CategoryAll;
        }

        CurrentPage = 1;
        _ = LoadProducts();
    }


    partial void OnMinPriceChanged(double? value)
    {
        CurrentPage = 1;
    }

    partial void OnMaxPriceChanged(double? value)
    {
        CurrentPage = 1;
    }

    [RelayCommand]
    private async Task GoToProductDetail(ProductListEntry product)
    {
        if (product == null) return;

        onEditProduct(product);
        await Task.CompletedTask;
    }

    [RelayCommand]
    private void GetAllCategories()
    {
        Categories.Clear();
        Categories.Add(CategoryAll);
        Categories.Add(CategoryCoffee);
        Categories.Add(CategoryTea);
        Categories.Add(CategorySmoothies);
        Categories.Add(CategoryPastries);
    }

    private static string BuildImportSuccessMessage(CatalogImportResult importResult)
    {
        if (importResult.InsertedProductNames.Count == 0)
        {
            return "Import thành công, nhưng không có sản phẩm mới được thêm.";
        }

        var previewNames = importResult.InsertedProductNames.Take(3).ToList();
        var productList = string.Join(", ", previewNames);

        if (importResult.InsertedProductNames.Count > previewNames.Count)
        {
            productList += $" và {importResult.InsertedProductNames.Count - previewNames.Count} sản phẩm khác";
        }

        return $"Đã thêm thành công {importResult.ProductsInserted} sản phẩm: {productList}.";
    }

    private static List<ProductListEntry> BuildSeedProducts()
    {
        return
        [
            new ProductListEntry { Id = 1, Code = "PRD-001", Name = "Bac xiu", Category = CategoryCoffee, CostPrice = 17000, SellingPrice = 25000 },
            new ProductListEntry { Id = 2, Code = "PRD-002", Name = "Ca phe sua da", Category = CategoryCoffee, CostPrice = 18000, SellingPrice = 25000 },
            new ProductListEntry { Id = 3, Code = "PRD-003", Name = "Tra sua truyen thong", Category = CategoryTea, CostPrice = 18000, SellingPrice = 25000 },
            new ProductListEntry { Id = 4, Code = "PRD-004", Name = "Matcha latte", Category = CategoryTea, CostPrice = 22000, SellingPrice = 32000 },
            new ProductListEntry { Id = 5, Code = "PRD-005", Name = "Mango smoothie", Category = CategorySmoothies, CostPrice = 26000, SellingPrice = 39000 },
            new ProductListEntry { Id = 6, Code = "PRD-006", Name = "Butter croissant", Category = CategoryPastries, CostPrice = 15000, SellingPrice = 28000 },
            new ProductListEntry { Id = 7, Code = "PRD-007", Name = "Americano", Category = CategoryCoffee, CostPrice = 16000, SellingPrice = 27000 }
        ];
    }
}
