using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Data;
using MyShop.Di;
using MyShop.Models;

namespace MyShop.ViewModels;

public partial class CategoryFormViewModel : ObservableObject
{
    private readonly IRepository<Category> categoryRepository;

    [ObservableProperty]
    private ObservableCollection<Category> categories = new();

    [ObservableProperty]
    private Category? selectedCategory;

    [ObservableProperty]
    private string categoryName = string.Empty;

    [ObservableProperty]
    private string categoryDescription = string.Empty;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public CategoryFormViewModel()
    {
        categoryRepository = ServiceContainer.Instance.Get<IRepository<Category>>();
        _ = ReloadCategories(); // Fire and forget for initialization
    }

    [RelayCommand]
    private async Task AddCategory()
    {
        var name = CategoryName.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            StatusMessage = "Category name is required.";
            return;
        }

        var exists = Categories.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (exists)
        {
            StatusMessage = "Category already exists.";
            return;
        }

        try
        {
            await categoryRepository.AddAsync(new Category
            {
                Name = name,
                Description = string.IsNullOrWhiteSpace(CategoryDescription) ? null : CategoryDescription.Trim()
            });

            ClearInputs();
            await ReloadCategories();
            StatusMessage = "Category added successfully.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error adding category: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task EditCategory()
    {
        if (SelectedCategory is null)
        {
            StatusMessage = "Select a category to edit.";
            return;
        }

        var name = CategoryName.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            StatusMessage = "Category name is required.";
            return;
        }

        var duplicate = Categories.Any(c =>
            c.Id != SelectedCategory.Id &&
            c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (duplicate)
        {
            StatusMessage = "Another category with the same name already exists.";
            return;
        }

        try
        {
            var category = await categoryRepository.GetByIdAsync(SelectedCategory.Id);
            if (category is null)
            {
                StatusMessage = "Category not found.";
                return;
            }

            category.Name = name;
            category.Description = string.IsNullOrWhiteSpace(CategoryDescription) ? null : CategoryDescription.Trim();
            await categoryRepository.UpdateAsync(category);

            await ReloadCategories();
            StatusMessage = "Category updated successfully.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error updating category: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteCategory()
    {
        if (SelectedCategory is null)
        {
            StatusMessage = "Select a category to delete.";
            return;
        }

        try
        {
            await categoryRepository.DeleteAsync(SelectedCategory.Id);
            SelectedCategory = null;
            ClearInputs();
            await ReloadCategories();
            StatusMessage = "Category deleted successfully.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error deleting category: {ex.Message}";
        }
    }

    partial void OnSelectedCategoryChanged(Category? value)
    {
        if (value is null)
        {
            return;
        }

        CategoryName = value.Name;
        CategoryDescription = value.Description ?? string.Empty;
    }

    private async Task ReloadCategories()
    {
        try
        {
            var result = await categoryRepository.GetAllAsync(null, null);
            var data = result.Items
                .OrderBy(c => c.Name)
                .ToList();

            Categories = new ObservableCollection<Category>(data);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading categories: {ex.Message}";
        }
    }

    private void ClearInputs()
    {
        CategoryName = string.Empty;
        CategoryDescription = string.Empty;
    }
}
