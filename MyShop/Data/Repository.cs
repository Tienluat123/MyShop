using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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

            // Nếu không có phân trang, lấy tất cả
            if (paging == null)
            {
                result.Items = _dbSet.ToList();
                result.Pagination = new PagingMetadata
                {
                    PageNumber = 1,
                    PageSize = result.Items.Count,
                    TotalItems = result.Items.Count,
                };
                return result;
            }

            // Validate phân trang
            paging.Validate();

            // Tính toán tổng mục
            var totalItems = _dbSet.Count();
            var totalPages = (int)Math.Ceiling((decimal)totalItems / paging.PageSize);

            // Lấy dữ liệu với phân trang
            var items = _dbSet
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
    /// Lấy IQueryable để thực hiện LINQ queries phức tạp
    /// </summary>
    public IQueryable<TEntity> Query()
    {
        return _dbSet.AsQueryable();
    }

    /// <summary>
    /// Tìm kiếm entities theo điều kiện
    /// </summary>
    public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
    {
        return _dbSet.Where(predicate).ToList();
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
