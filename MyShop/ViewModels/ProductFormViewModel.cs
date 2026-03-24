using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MyShop.ViewModels;

public partial class ProductFormViewModel : ObservableObject
{
    private readonly Action onCancel;

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

    public ObservableCollection<string> Categories { get; } = new();

    public ProductFormViewModel(bool isEditMode, ProductListEntry? product, Action onCancel)
    {
        this.onCancel = onCancel;

        Categories.Add("Coffee");
        Categories.Add("Tea");
        Categories.Add("Smoothies");
        Categories.Add("Pastries");

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
                Category = "Coffee"
            };
        }
    }

    [RelayCommand]
    private void Submit()
    {
        onCancel();
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

        if (!Categories.Contains(categoryName))
        {
            Categories.Add(categoryName);
        }

        ProductData.Category = categoryName;
        NewCategoryName = string.Empty;
        OnPropertyChanged(nameof(ProductData));
    }
}
