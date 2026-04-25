using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Data;
using MyShop.Di;
using MyShop.Models;
using MyShop.Services;

namespace MyShop.ViewModels;

public partial class ProductFormViewModel : ObservableObject
{
    private readonly Action onCancel;
    private readonly Action onSaved;
    private readonly ProductListEntry? editingProduct;
    private readonly IProductService productService;
    private readonly IRepository<Product> productRepository;
    private readonly IRepository<Category> categoryRepository;

    [ObservableProperty]
    private bool isEditMode;

    [ObservableProperty]
    private string formTitle = "New Product";

    [ObservableProperty]
    private string subtitle = "Add a product";

    [ObservableProperty]
    private string submitButtonText = "SAVE";

    [ObservableProperty]
    private ProductInputData productData = new();

    [ObservableProperty]
    private string newCategoryName = string.Empty;

    [ObservableProperty]
    private string validationMessage = string.Empty;

    [ObservableProperty]
    private bool isFormValid;

    public ObservableCollection<string> Categories { get; } = new();

    public ProductFormViewModel(bool isEditMode, ProductListEntry? product, Action onCancel, Action? onSaved = null)
    {
        var container = ServiceContainer.Instance;
        this.onCancel = onCancel;
        this.onSaved = onSaved ?? (() => { });
        editingProduct = product;
        productService = container.Get<IProductService>();
        productRepository = container.Get<IRepository<Product>>();
        categoryRepository = container.Get<IRepository<Category>>();

        LoadCategoriesFromRepository();

        IsEditMode = isEditMode;
        if (isEditMode && product is not null)
        {
            FormTitle = "Edit Product";
            Subtitle = "Update product";
            SubmitButtonText = "SAVE CHANGES";
            ProductData = new ProductInputData
            {
                Name = product.Name,
                Category = product.Category,
                CostPrice = product.CostPrice.ToString("N0"),
                SellingPrice = product.SellingPrice.ToString("N0")
            };
        }
        else
        {
            FormTitle = "New Product";
            Subtitle = "Add a product";
            SubmitButtonText = "SAVE";
            ProductData = new ProductInputData
            {
                Category = Categories.FirstOrDefault() ?? "Coffee"
            };
        }
    }

    [RelayCommand]
    private void Submit()
    {
        SaveProduct();
    }

    [RelayCommand]
    private void Cancel()
    {
        onCancel();
    }

    [RelayCommand]
    private void AddCategory()
    {
        var categoryName = NewCategoryName?.Trim();
        if (string.IsNullOrWhiteSpace(categoryName))
        {
            return;
        }

        if (!Categories.Any(c => c.Equals(categoryName, StringComparison.OrdinalIgnoreCase)))
        {
            Categories.Add(categoryName);
        }

        ProductData.Category = categoryName;
        NewCategoryName = string.Empty;
        OnPropertyChanged(nameof(ProductData));
    }

    [RelayCommand]
    private void SaveProduct()
    {
        try
        {
            var (isValid, errorMessage) = ValidateFormData();
            IsFormValid = isValid;
            ValidationMessage = errorMessage;

            if (!isValid)
            {
                return;
            }

            var categoryName = ProductData.Category.Trim();
            var category = categoryRepository.GetAll().Items
                .FirstOrDefault(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));

            if (category is null)
            {
                category = categoryRepository.Add(new Category
                {
                    Name = categoryName
                });
            }

            if (!decimal.TryParse(ProductData.SellingPrice, NumberStyles.Number, CultureInfo.CurrentCulture, out var sellingPrice))
            {
                IsFormValid = false;
                ValidationMessage = "Selling price is invalid.";
                return;
            }

            if (IsEditMode && editingProduct is not null)
            {
                var existing = productRepository.GetById(editingProduct.Id);
                if (existing is null)
                {
                    IsFormValid = false;
                    ValidationMessage = "Product not found for update.";
                    return;
                }

                existing.Name = ProductData.Name.Trim();
                existing.CategoryId = category.Id;
                existing.Price = sellingPrice;
                existing.ImagePath = string.IsNullOrWhiteSpace(ProductData.ImageUrl) ? existing.ImagePath : ProductData.ImageUrl;
                productService.Update(existing);
            }
            else
            {
                productService.Add(new Product
                {
                    Name = ProductData.Name.Trim(),
                    CategoryId = category.Id,
                    Price = sellingPrice,
                    ImagePath = string.IsNullOrWhiteSpace(ProductData.ImageUrl) ? null : ProductData.ImageUrl
                });
            }

            onSaved();
            onCancel();
        }
        catch (Exception ex)
        {
            IsFormValid = false;
            ValidationMessage = $"Save failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ValidateProductForm()
    {
        var (isValid, errorMessage) = ValidateFormData();
        IsFormValid = isValid;
        ValidationMessage = errorMessage;
    }

    [RelayCommand]
    private void UploadProductImage()
    {
        var uploadRoot = Path.Combine(AppContext.BaseDirectory, "uploads", "products");
        Directory.CreateDirectory(uploadRoot);

        var filePath = Path.Combine(uploadRoot, $"product-{DateTime.UtcNow:yyyyMMddHHmmssfff}.jpg");
        ProductData.ImageUrl = filePath;
        ValidationMessage = "Image path is generated. Integrate file picker to store real file content.";
        OnPropertyChanged(nameof(ProductData));
    }

    private (bool IsValid, string ErrorMessage) ValidateFormData()
    {
        if (string.IsNullOrWhiteSpace(ProductData.Name))
        {
            return (false, "Product name is required.");
        }

        if (string.IsNullOrWhiteSpace(ProductData.Category))
        {
            return (false, "Category is required.");
        }

        if (!decimal.TryParse(ProductData.CostPrice, NumberStyles.Number, CultureInfo.CurrentCulture, out var cost))
        {
            return (false, "Cost price is invalid.");
        }

        if (!decimal.TryParse(ProductData.SellingPrice, NumberStyles.Number, CultureInfo.CurrentCulture, out var price))
        {
            return (false, "Selling price is invalid.");
        }

        if (cost < 0 || price < 0)
        {
            return (false, "Prices cannot be negative.");
        }

        if (price <= 0)
        {
            return (false, "Selling price must be greater than 0.");
        }

        if (price < cost)
        {
            return (false, "Selling price must be greater than or equal to cost price.");
        }

        return (true, string.Empty);
    }

    private void LoadCategoriesFromRepository()
    {
        Categories.Clear();

        var allCategories = categoryRepository.GetAll().Items
            .Where(c => !string.IsNullOrWhiteSpace(c.Name))
            .Select(c => c.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name)
            .ToList();

        if (allCategories.Count == 0)
        {
            Categories.Add("Coffee");
            Categories.Add("Tea");
            Categories.Add("Smoothies");
            Categories.Add("Pastries");
            return;
        }

        foreach (var category in allCategories)
        {
            Categories.Add(category);
        }
    }
}
