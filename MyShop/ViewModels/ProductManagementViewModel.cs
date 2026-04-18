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
        Action onAddProduct,
        Action<ProductListEntry> onEditProduct)
    {

        _productService = productService;

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
            const int pageSize = 3;

            // Load all first, then apply local filter/sort/paging to avoid double paging.
            var result = await _productService.GetAll();

            IEnumerable<Product> query = result.Items;

            // FILTER CATEGORY
            if (SelectedCategory != CategoryAll)
            {
                query = query.Where(p => p.Category?.Name == SelectedCategory);
            }

            // SEARCH
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                query = query.Where(p => p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            // PRICE
            if (MinPrice.HasValue)
                query = query.Where(p => p.Price >= (decimal)MinPrice.Value);

            if (MaxPrice.HasValue)
                query = query.Where(p => p.Price <= (decimal)MaxPrice.Value);

            // SORT
            query = SortOption switch
            {
                "PriceAsc" => query.OrderBy(p => p.Price),
                "PriceDesc" => query.OrderByDescending(p => p.Price),
                _ => query.OrderBy(p => p.Name)
            };

            var list = query.ToList();

            TotalItems = list.Count;
            TotalPages = Math.Max(1, (int)Math.Ceiling(list.Count / (double)pageSize));

            if (CurrentPage > TotalPages)
                CurrentPage = TotalPages;

            var pageItems = list
                .Skip((CurrentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            PageNumbers.Clear();
            for (int i = 1; i <= TotalPages; i++)
                PageNumbers.Add(i);

            DisplayProducts.Clear();
            foreach (var p in pageItems)
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
            var pageStart = TotalItems == 0 ? 0 : (CurrentPage - 1) * pageSize + 1;
            var pageEnd = pageStart + pageItems.Count - 1;
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
