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
    /// Lấy tất cả entities với phân trang (Bất đồng bộ)
    /// </summary>
    public async Task<PagedResult<TEntity>> GetAll(PagingRequest? paging = null)
    {
        try
        {
            var result = new PagedResult<TEntity>();

            // Nếu không có phân trang, lấy tất cả
            if (paging == null)
            {
                result.Items = await _dbSet.ToListAsync();
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
            var totalItems = await _dbSet.CountAsync();

            // Lấy dữ liệu với phân trang
            var items = await _dbSet
                .Skip(paging.GetSkip())
                .Take(paging.PageSize)
                .ToListAsync();

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
    /// Lấy entity theo ID (Bất đồng bộ)
    /// </summary>
    public async Task<TEntity?> GetById(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID phải lớn hơn 0", nameof(id));

        try
        {
            return await _dbSet.FindAsync(id);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi lấy {typeof(TEntity).Name} với ID {id}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Thêm entity mới (Bất đồng bộ)
    /// </summary>
    public async Task<TEntity> Add(TEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        try
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi thêm {typeof(TEntity).Name}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Cập nhật entity (Bất đồng bộ)
    /// </summary>
    public async Task Update(TEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        try
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi cập nhật {typeof(TEntity).Name}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Xóa entity theo ID (Bất đồng bộ)
    /// </summary>
    public async Task Delete(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID phải lớn hơn 0", nameof(id));

        try
        {
            var entity = await GetById(id);
            if (entity == null)
                throw new KeyNotFoundException($"{typeof(TEntity).Name} với ID {id} không tìm thấy");

            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
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
    /// Tìm kiếm entities theo điều kiện (Bất đồng bộ)
    /// </summary>
    public async Task<IEnumerable<TEntity>> Find(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    /// <summary>
    /// Lưu tất cả thay đổi vào database (Bất đồng bộ)
    /// </summary>
    public async Task SaveChanges()
    {
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi lưu dữ liệu: {ex.Message}", ex);
        }
    }
}