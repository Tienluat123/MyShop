using MyShop.Data;
using MyShop.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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
    Task<IEnumerable<Product>> GetLowStockProducts(int threshold = 5, int limit = 5);
    Task<int> GetTotalCount();
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
      

    public async Task<IEnumerable<Product>> GetLowStockProducts(int threshold = 5, int limit = 5)
    {
        return await _productRepository.ExecuteAsync(async context =>
        {
            return await context.Set<Product>()
                .Include(p => p.Category)
                .Where(p => p.StockQuantity < threshold)
                .OrderBy(p => p.StockQuantity)
                .Take(limit)
                .ToListAsync();
        }, "GetLowStockProducts");
    }
    /// <summary>
    /// Lấy tất cả sản phẩm (Bất đồng bộ)
    /// ✅ BƯỚC 2: Sử dụng GetAsync với Include thay vì Query()
    /// </summary>
    public async Task<PagedResult<Product>> GetAll(PagingRequest? paging = null)
    {
        try
        {
            Expression<Func<Product, bool>>? filter = null;

            if (paging != null)
            {
                var filters = new List<Expression<Func<Product, bool>>>();

                // SearchText filter
                if (!string.IsNullOrWhiteSpace(paging.SearchText))
                {
                    var keyword = paging.SearchText.Trim();
                    // Kết hợp PostgreSQL Full-Text Search (ToTsVector) và Substring Matching (ILike)
                    // FTS giúp tìm không cần đúng thứ tự từ (VD: "sữa cafe" tìm ra "cafe sữa")
                    // ILike giúp tìm chuỗi con (VD: "caf" tìm ra "cafe")
                    filters.Add(p =>
                        EF.Functions.ToTsVector("simple", p.Name).Matches(EF.Functions.PlainToTsQuery("simple", keyword)) ||
                        EF.Functions.ILike(p.Name, $"%{keyword}%") ||
                        (p.Category != null && EF.Functions.ILike(p.Category.Name, $"%{keyword}%"))
                    );
                }

                // Category filter from Dictionary
                if (paging.Filters != null && paging.Filters.TryGetValue("Category", out var category) && !string.IsNullOrWhiteSpace(category) && category != "All")
                {
                    filters.Add(p => p.Category != null && p.Category.Name == category);
                }

                // Price range filters
                if (paging.Filters != null && paging.Filters.TryGetValue("MinPrice", out var minPriceStr) && decimal.TryParse(minPriceStr, out var minPrice))
                {
                    filters.Add(p => p.Price >= minPrice);
                }

                if (paging.Filters != null && paging.Filters.TryGetValue("MaxPrice", out var maxPriceStr) && decimal.TryParse(maxPriceStr, out var maxPrice))
                {
                    filters.Add(p => p.Price <= maxPrice);
                }

                // Combine filters
                if (filters.Any())
                {
                    var parameter = Expression.Parameter(typeof(Product), "p");
                    Expression? combined = null;

                    foreach (var f in filters)
                    {
                        var body = f.Body;
                        // Replace parameter to maintain consistency
                        body = new ParameterReplacer(f.Parameters[0], parameter).Visit(body);

                        combined = combined == null ? body : Expression.AndAlso(combined, body);
                    }

                    if (combined != null)
                    {
                        filter = Expression.Lambda<Func<Product, bool>>(combined, parameter);
                    }
                }
            }

            if (paging != null)
            {
                paging.SortBy = NormalizeProductSortBy(paging.SortBy);
            }

            return await _productRepository.GetAllAsync(paging, filter, "Category", "ProductImages");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Lỗi khi lấy danh sách sản phẩm", ex);
        }
    }

    private class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParameter;
        private readonly ParameterExpression _newParameter;

        public ParameterReplacer(ParameterExpression oldParameter, ParameterExpression newParameter)
        {
            _oldParameter = oldParameter;
            _newParameter = newParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _oldParameter ? _newParameter : base.VisitParameter(node);
        }
    }

    private static string? NormalizeProductSortBy(string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return null;
        }

        var allowedColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Name"] = nameof(Product.Name),
            ["Price"] = nameof(Product.Price),
            ["Category"] = $"{nameof(Product.Category)}.{nameof(Category.Name)}"
        };

        var sortSegments = sortBy
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(segment =>
            {
                var parts = segment.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length == 0 || !allowedColumns.TryGetValue(parts[0], out var propertyPath))
                {
                    return null;
                }

                var direction = parts.Length > 1 && parts[1].Equals("DESC", StringComparison.OrdinalIgnoreCase)
                    ? "DESC"
                    : "ASC";

                return $"{propertyPath} {direction}";
            })
            .Where(segment => !string.IsNullOrWhiteSpace(segment))
            .ToList();

        return sortSegments.Count == 0 ? null : string.Join(", ", sortSegments);
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
            var items = await _productRepository.GetAsync(p => p.Id == id, "Category", "ProductImages");
            return items.FirstOrDefault();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi lấy sản phẩm với ID {id}", ex);
        }
    }

    /// <summary>
    /// Lấy sản phẩm theo category (Bất đồng bộ)
    /// ✅ BƯỚC 2: Sử dụng GetAsync với filter server-side
    /// </summary>
    public async Task<List<Product>> GetByCategory(int categoryId)
    {
        if (categoryId <= 0)
            throw new ArgumentException("Category ID phải lớn hơn 0", nameof(categoryId));

        try
        {
            return await _productRepository.GetAsync(
                p => p.CategoryId == categoryId,
                "Category",
                "ProductImages");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi lấy sản phẩm theo category {categoryId}", ex);
        }
    }

    /// <summary>
    /// Tìm kiếm sản phẩm (Bất đồng bộ)
    /// ✅ BƯỚC 2: Sử dụng GetAsync với filter server-side
    /// </summary>
    public async Task<List<Product>> Search(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return new List<Product>();

        try
        {
            var keywordLower = keyword.ToLower();
            return await _productRepository.GetAsync(
                p => p.Name.ToLower().Contains(keywordLower),
                "Category",
                "ProductImages");
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

        var category = await _categoryRepository.GetByIdAsync(product.CategoryId);
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
    public async Task<int> GetTotalCount()
    {
        return await _productRepository.CountAsync();
    }
}
