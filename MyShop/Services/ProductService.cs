using MyShop.Data;
using MyShop.Models;

namespace MyShop.Services;

/// <summary>
/// Interface cho Product Service - định nghĩa business logic cho Product
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Lấy tất cả sản phẩm với phân trang
    /// </summary>
    PagedResult<Product> GetAll(PagingRequest? paging = null);

    /// <summary>
    /// Lấy sản phẩm theo ID
    /// </summary>
    Product? GetById(int id);

    /// <summary>
    /// Lấy sản phẩm theo category ID
    /// </summary>
    List<Product> GetByCategory(int categoryId);

    /// <summary>
    /// Tìm kiếm sản phẩm theo tên
    /// </summary>
    List<Product> Search(string keyword);

    /// <summary>
    /// Thêm sản phẩm mới
    /// </summary>
    Product Add(Product product);

    /// <summary>
    /// Cập nhật sản phẩm
    /// </summary>
    void Update(Product product);

    /// <summary>
    /// Xóa sản phẩm
    /// </summary>
    void Delete(int id);
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
    /// Lấy tất cả sản phẩm
    /// </summary>
    public PagedResult<Product> GetAll(PagingRequest? paging = null)
    {
        try
        {
            return _productRepository.GetAll(paging);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Lỗi khi lấy danh sách sản phẩm", ex);
        }
    }

    /// <summary>
    /// Lấy sản phẩm theo ID
    /// </summary>
    public Product? GetById(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID phải lớn hơn 0", nameof(id));

        try
        {
            return _productRepository.GetById(id);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi lấy sản phẩm với ID {id}", ex);
        }
    }

    /// <summary>
    /// Lấy sản phẩm theo category
    /// </summary>
    public List<Product> GetByCategory(int categoryId)
    {
        if (categoryId <= 0)
            throw new ArgumentException("Category ID phải lớn hơn 0", nameof(categoryId));

        try
        {
            // ✅ Thực tế sẽ dùng LINQ to Entities, nhưng hiện tại dùng simple filter
            var allProducts = _productRepository.GetAll().Items;
            return allProducts
                .Where(p => p.CategoryId == categoryId)
                .ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi lấy sản phẩm theo category {categoryId}", ex);
        }
    }

    /// <summary>
    /// Tìm kiếm sản phẩm
    /// </summary>
    public List<Product> Search(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return new List<Product>();

        try
        {
            var allProducts = _productRepository.GetAll().Items;
            return allProducts
                .Where(p => p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi tìm kiếm sản phẩm với từ khóa '{keyword}'", ex);
        }
    }

    /// <summary>
    /// Thêm sản phẩm mới
    /// </summary>
    public Product Add(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        // ✅ Business logic validation
        if (string.IsNullOrWhiteSpace(product.Name))
            throw new ArgumentException("Tên sản phẩm không được để trống", nameof(product.Name));

        if (product.Price < 0)
            throw new ArgumentException("Giá sản phẩm không được âm", nameof(product.Price));

        if (product.CategoryId <= 0)
            throw new ArgumentException("Phải chọn category hợp lệ", nameof(product.CategoryId));

        // Kiểm tra category tồn tại
        var category = _categoryRepository.GetById(product.CategoryId);
        if (category == null)
            throw new InvalidOperationException($"Category với ID {product.CategoryId} không tồn tại");

        try
        {
            return _productRepository.Add(product);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Lỗi khi thêm sản phẩm", ex);
        }
    }

    /// <summary>
    /// Cập nhật sản phẩm
    /// </summary>
    public void Update(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        if (product.Id <= 0)
            throw new ArgumentException("ID sản phẩm không hợp lệ", nameof(product.Id));

        // ✅ Business logic validation
        if (string.IsNullOrWhiteSpace(product.Name))
            throw new ArgumentException("Tên sản phẩm không được để trống", nameof(product.Name));

        if (product.Price < 0)
            throw new ArgumentException("Giá sản phẩm không được âm", nameof(product.Price));

        try
        {
            _productRepository.Update(product);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Lỗi khi cập nhật sản phẩm", ex);
        }
    }

    /// <summary>
    /// Xóa sản phẩm
    /// </summary>
    public void Delete(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID sản phẩm không hợp lệ", nameof(id));

        try
        {
            _productRepository.Delete(id);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi xóa sản phẩm với ID {id}", ex);
        }
    }
}
