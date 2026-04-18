using System;
using System.Collections.ObjectModel;
using System.Linq;
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
        ReloadCategories();
    }

    [RelayCommand]
    private void AddCategory()
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

        categoryRepository.Add(new Category
        {
            Name = name,
            Description = string.IsNullOrWhiteSpace(CategoryDescription) ? null : CategoryDescription.Trim()
        });

        ClearInputs();
        ReloadCategories();
        StatusMessage = "Category added successfully.";
    }

    [RelayCommand]
    private void EditCategory()
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

        var category = categoryRepository.GetById(SelectedCategory.Id);
        if (category is null)
        {
            StatusMessage = "Category not found.";
            return;
        }

        category.Name = name;
        category.Description = string.IsNullOrWhiteSpace(CategoryDescription) ? null : CategoryDescription.Trim();
        categoryRepository.Update(category);

        ReloadCategories();
        StatusMessage = "Category updated successfully.";
    }

    [RelayCommand]
    private void DeleteCategory()
    {
        if (SelectedCategory is null)
        {
            StatusMessage = "Select a category to delete.";
            return;
        }

        categoryRepository.Delete(SelectedCategory.Id);
        SelectedCategory = null;
        ClearInputs();
        ReloadCategories();
        StatusMessage = "Category deleted successfully.";
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

    private void ReloadCategories()
    {
        var data = categoryRepository.GetAll().Items
            .OrderBy(c => c.Name)
            .ToList();

        Categories = new ObservableCollection<Category>(data);
    }

    private void ClearInputs()
    {
        CategoryName = string.Empty;
        CategoryDescription = string.Empty;
    }
}
