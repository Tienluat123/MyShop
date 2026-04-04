using MyShop.Data;
using MyShop.Models;

namespace MyShop.Services;

/// <summary>
/// Interface for Category Service - defines business logic for Categories
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// Gets all categories
    /// </summary>
    Task<List<Category>> GetAll();

    /// <summary>
    /// Gets category by ID
    /// </summary>
    Task<Category?> GetById(int id);
}

/// <summary>
/// Category Service Implementation
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly IRepository<Category> _categoryRepository;

    public CategoryService(IRepository<Category> categoryRepository)
    {
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
    }

    /// <summary>
    /// Gets all categories from the repository
    /// </summary>
    public async Task<List<Category>> GetAll()
    {
        try
        {
            var result = await _categoryRepository.GetAll();
            return result.Items;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error fetching categories", ex);
        }
    }

    /// <summary>
    /// Gets category by ID
    /// </summary>
    public async Task<Category?> GetById(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID must be greater than 0", nameof(id));

        try
        {
            return await _categoryRepository.GetById(id);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error fetching category with ID {id}", ex);
        }
    }
}
