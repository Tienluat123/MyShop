using MyShop.Data;
using MyShop.Models;
using Microsoft.EntityFrameworkCore;

namespace MyShop.Services;

/// <summary>
/// Interface cho Product Service - định nghĩa business logic cho Product
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Lấy tất cả sản phẩm với phân trang
    /// </summary>
    Task<PagedResult<Product>> GetAll(PagingRequest? paging = null);

    /// <summary>
    /// Lấy sản phẩm theo ID
    /// </summary>
    Task<Product?> GetById(int id);

    /// <summary>
    /// Lấy sản phẩm theo category ID
    /// </summary>
    Task<List<Product>> GetByCategory(int categoryId);

    /// <summary>
    /// Tìm kiếm sản phẩm theo tên
    /// </summary>
    Task<List<Product>> Search(string keyword);

    /// <summary>
    /// Thêm sản phẩm mới
    /// </summary>
    Task<Product> Add(Product product);

    /// <summary>
    /// Cập nhật sản phẩm
    /// </summary>
    Task Update(Product product);

    /// <summary>
    /// Xóa sản phẩm
    /// </summary>
    Task Delete(int id);
}

/// <summary>
/// Product Service Implementation
/// </summary>
public class ProductService : IProductService
{
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Category> _categoryRepository;

    public ProductService(
        IRepository<Product> productRepository,
        IRepository<Category> categoryRepository)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
    }

    /// <summary>
    /// Lấy tất cả sản phẩm (Bất đồng bộ)
    /// </summary>
    public async Task<PagedResult<Product>> GetAll(PagingRequest? paging = null)
    {
        try
        {
            var query = _productRepository.Query().Include(p => p.Category);
            var result = new PagedResult<Product>();

            if (paging == null)
            {
                result.Items = await query.ToListAsync();
                result.Pagination = new PagingMetadata
                {
                    PageNumber = 1,
                    PageSize = result.Items.Count,
                    TotalItems = result.Items.Count,
                };
            }
            else
            {
                paging.Validate();
                var totalItems = await query.CountAsync();
                var items = await query.Skip(paging.GetSkip()).Take(paging.PageSize).ToListAsync();

                result.Items = items;
                result.Pagination = new PagingMetadata
                {
                    PageNumber = paging.PageNumber,
                    PageSize = paging.PageSize,
                    TotalItems = totalItems,
                };
            }
            return result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Lỗi khi lấy danh sách sản phẩm", ex);
        }
    }

    /// <summary>
    /// Lấy sản phẩm theo ID (Bất đồng bộ)
    /// </summary>
    public async Task<Product?> GetById(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID phải lớn hơn 0", nameof(id));

        try
        {
            var product = _productRepository.GetById(id);
            return product;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi lấy sản phẩm với ID {id}", ex);
        }
    }

    /// <summary>
    /// Lấy sản phẩm theo category (Bất đồng bộ)
    /// </summary>
    public async Task<List<Product>> GetByCategory(int categoryId)
    {
        if (categoryId <= 0)
            throw new ArgumentException("Category ID phải lớn hơn 0", nameof(categoryId));

        try
        {
            return await _productRepository.Query()
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi lấy sản phẩm theo category {categoryId}", ex);
        }
    }

    /// <summary>
    /// Tìm kiếm sản phẩm (Bất đồng bộ)
    /// </summary>
    public async Task<List<Product>> Search(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return new List<Product>();

        try
        {
            return await _productRepository.Query()
                .Include(p => p.Category)
                .Where(p => p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi tìm kiếm sản phẩm với từ khóa '{keyword}'", ex);
        }
    }

    /// <summary>
    /// Thêm sản phẩm mới (Bất đồng bộ)
    /// </summary>
    public async Task<Product> Add(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        if (string.IsNullOrWhiteSpace(product.Name))
            throw new ArgumentException("Tên sản phẩm không được để trống", nameof(product.Name));

        if (product.Price < 0)
            throw new ArgumentException("Giá sản phẩm không được âm", nameof(product.Price));

        if (product.CategoryId <= 0)
            throw new ArgumentException("Phải chọn category hợp lệ", nameof(product.CategoryId));

        var category = _categoryRepository.GetById(product.CategoryId);
        if (category == null)
            throw new InvalidOperationException($"Category với ID {product.CategoryId} không tồn tại");

        try
        {
            return await _productRepository.AddAsync(product);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Lỗi khi thêm sản phẩm", ex);
        }
    }

    /// <summary>
    /// Cập nhật sản phẩm (Bất đồng bộ)
    /// </summary>
    public async Task Update(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        if (product.Id <= 0)
            throw new ArgumentException("ID sản phẩm không hợp lệ", nameof(product.Id));

        if (string.IsNullOrWhiteSpace(product.Name))
            throw new ArgumentException("Tên sản phẩm không được để trống", nameof(product.Name));

        if (product.Price < 0)
            throw new ArgumentException("Giá sản phẩm không được âm", nameof(product.Price));

        try
        {
            await _productRepository.UpdateAsync(product);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Lỗi khi cập nhật sản phẩm", ex);
        }
    }

    /// <summary>
    /// Xóa sản phẩm (Bất đồng bộ)
    /// </summary>
    public async Task Delete(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID sản phẩm không hợp lệ", nameof(id));

        try
        {
            await _productRepository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi xóa sản phẩm với ID {id}", ex);
        }
    }
}