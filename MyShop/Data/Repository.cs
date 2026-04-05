using Microsoft.EntityFrameworkCore;
using MyShop.Models;
namespace MyShop.Data;

/// <summary>
/// Generic Repository implementation - cung cấp CRUD operations cho bất kỳ Entity nào
/// </summary>
/// <typeparam name="TEntity">Kiểu Entity</typeparam>
public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    private readonly AppDbContext _context;
    private readonly DbSet<TEntity> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<TEntity>();
    }

    /// <summary>
    /// Lấy tất cả entities với phân trang
    /// </summary>
    public PagedResult<TEntity> GetAll(PagingRequest? paging = null)
{
    try
    {
        var result = new PagedResult<TEntity>();
        // 1. Tạo query ban đầu từ DbSet
        var query = _dbSet.AsQueryable();

        if (paging == null)
        {
            result.Items = query.ToList();
            result.Pagination = new PagingMetadata
            {
                PageNumber = 1,
                PageSize = result.Items.Count,
                TotalItems = result.Items.Count,
            };
            return result;
        }

        // 2. CHỖ THÊM SEARCH: Lọc dữ liệu theo SearchText
        if (!string.IsNullOrEmpty(paging.SearchText))
        {
            var search = paging.SearchText.ToLower();
            
            // Vì đây là Repository Generic, ta kiểm tra nếu là Customer thì lọc theo Name/Phone
            if (typeof(TEntity) == typeof(Customer))
            {
                query = query.Where(e => 
                    (e as Customer).Name.ToLower().Contains(search) || 
                    (e as Customer).Phone.Contains(search));
            }
            // Sau này nếu có Product, bạn có thể thêm else if ở đây
        }

        paging.Validate();

        // 3. QUAN TRỌNG: Phải Count() trên cái query đã được lọc
        var totalItems = query.Count();

        // 4. QUAN TRỌNG: Phải Skip/Take trên cái query đã được lọc
        var items = query
            .Skip(paging.GetSkip())
            .Take(paging.PageSize)
            .ToList();

        result.Items = items;
        result.Pagination = new PagingMetadata
        {
            PageNumber = paging.PageNumber,
            PageSize = paging.PageSize,
            TotalItems = totalItems,
        };

        return result;
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException($"Lỗi khi lấy dữ liệu {typeof(TEntity).Name}: {ex.Message}", ex);
    }
}
    /// <summary>
    /// Lấy entity theo ID
    /// </summary>
    public TEntity? GetById(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID phải lớn hơn 0", nameof(id));

        try
        {
            return _dbSet.Find(id);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi lấy {typeof(TEntity).Name} với ID {id}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Thêm entity mới
    /// </summary>
    public TEntity Add(TEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        try
        {
            _dbSet.Add(entity);
            _context.SaveChanges();
            return entity;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi thêm {typeof(TEntity).Name}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Cập nhật entity
    /// </summary>
    public void Update(TEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        try
        {
            _dbSet.Update(entity);
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi cập nhật {typeof(TEntity).Name}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Xóa entity theo ID
    /// </summary>
    public void Delete(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID phải lớn hơn 0", nameof(id));

        try
        {
            var entity = GetById(id);
            if (entity == null)
                throw new KeyNotFoundException($"{typeof(TEntity).Name} với ID {id} không tìm thấy");

            _dbSet.Remove(entity);
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi xóa {typeof(TEntity).Name} với ID {id}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Lưu tất cả thay đổi vào database
    /// </summary>
    public void SaveChanges()
    {
        try
        {
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi lưu dữ liệu: {ex.Message}", ex);
        }
    }
}
