using MyShop.Models;
using MyShop.Services;
using MyShop.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MyShop.ViewModels;

public partial class ProductManagementViewModel : ObservableObject
{
    private readonly IProductService _productService;
    private readonly IAppConfigService _configService;

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
    private string sortOption = "Name"; // Name | PriceAsc | PriceDesc

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool hasNoData;

    public ProductManagementViewModel(
        IProductService productService,
        IAppConfigService configService,
        Action onAddProduct,
        Action<ProductListEntry> onEditProduct)
    {
        _productService = productService;
        _configService = configService;

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
                SortBy = SortOption switch
                {
                    "PriceAsc" => "Price ASC",
                    "PriceDesc" => "Price DESC",
                    _ => "Name ASC"
                }
            };

            var result = await _productService.GetAll(request);

            TotalItems = result.Pagination.TotalItems;
            TotalPages = result.Pagination.TotalPages;

            if (CurrentPage > TotalPages)
                CurrentPage = TotalPages;

            PageNumbers.Clear();
            for (int i = 1; i <= TotalPages; i++)
                PageNumbers.Add(i);

            DisplayProducts.Clear();
            foreach (var p in result.Items)
            {
                DisplayProducts.Add(new ProductListEntry
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = p.Category?.Name ?? "",
                    SellingPrice = p.Price
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
    private async Task SortProducts(string sortBy)
    {
        SortOption = sortBy;
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

    partial void OnSortOptionChanged(string value)
    {
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
