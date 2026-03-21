using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MyShop.ViewModels;

public partial class ProductManagementViewModel : ObservableObject
{
    private const string CategoryAll = "All";
    private const string CategoryCoffee = "Coffee";
    private const string CategoryTea = "Tea";
    private const string CategorySmoothies = "Smoothies";
    private const string CategoryPastries = "Pastries";

    private readonly Action onAddProduct;
    private readonly Action<ProductListEntry> onEditProduct;
    private readonly List<ProductListEntry> allProducts;

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

    public ProductManagementViewModel(Action onAddProduct, Action<ProductListEntry> onEditProduct)
    {
        this.onAddProduct = onAddProduct;
        this.onEditProduct = onEditProduct;

        Categories.Add(CategoryAll);
        Categories.Add(CategoryCoffee);
        Categories.Add(CategoryTea);
        Categories.Add(CategorySmoothies);
        Categories.Add(CategoryPastries);

        allProducts = BuildSeedProducts();
        RebuildList();
    }

    [RelayCommand]
    private void SelectCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return;
        }

        SelectedCategory = category;
        CurrentPage = 1;
        RebuildList();
    }

    [RelayCommand]
    private void Search()
    {
        CurrentPage = 1;
        RebuildList();
    }

    [RelayCommand]
    private void GoToPage(int page)
    {
        if (page < 1 || page > TotalPages)
        {
            return;
        }

        CurrentPage = page;
        RebuildList();
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (CurrentPage <= 1)
        {
            return;
        }

        CurrentPage--;
        RebuildList();
    }

    [RelayCommand]
    private void NextPage()
    {
        if (CurrentPage >= TotalPages)
        {
            return;
        }

        CurrentPage++;
        RebuildList();
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

    private void RebuildList()
    {
        const int pageSize = 3;

        var filtered = allProducts.Where(p =>
            (SelectedCategory == CategoryAll || p.Category.Equals(SelectedCategory, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrWhiteSpace(SearchText) || p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase))).ToList();

        TotalItems = filtered.Count;
        TotalPages = Math.Max(1, (int)Math.Ceiling(filtered.Count / (double)pageSize));

        if (CurrentPage > TotalPages)
        {
            CurrentPage = TotalPages;
        }

        var pageItems = filtered.Skip((CurrentPage - 1) * pageSize).Take(pageSize).ToList();

        DisplayProducts.Clear();
        foreach (var product in pageItems)
        {
            DisplayProducts.Add(product);
        }

        PageNumbers.Clear();
        for (int i = 1; i <= TotalPages; i++)
        {
            PageNumbers.Add(i);
        }

        var start = filtered.Count == 0 ? 0 : ((CurrentPage - 1) * pageSize) + 1;
        var end = filtered.Count == 0 ? 0 : start + pageItems.Count - 1;
        PagingSummary = $"Showing {start}-{end} in {TotalItems} items";
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
