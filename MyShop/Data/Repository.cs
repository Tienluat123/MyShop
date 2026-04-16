using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MyShop.Data;

/// <summary>
/// Generic Repository implementation - cung cấp CRUD operations cho bất kỳ Entity nào
/// ✅ Thread-safe: Uses AppDbContextFactory to create fresh context for each operation
/// </summary>
/// <typeparam name="TEntity">Kiểu Entity</typeparam>
public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    private readonly AppDbContextFactory _contextFactory;

    public Repository(AppDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
    }

    /// <summary>
    /// Lấy tất cả entities với phân trang
    /// </summary>
    public PagedResult<TEntity> GetAll(PagingRequest? paging = null)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext(Array.Empty<string>());
            var dbSet = context.Set<TEntity>();
            var result = new PagedResult<TEntity>();

            // Nếu không có phân trang, lấy tất cả
            if (paging == null)
            {
                result.Items = dbSet.ToList();
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
            var totalItems = dbSet.Count();
            var totalPages = (int)Math.Ceiling((decimal)totalItems / paging.PageSize);

            // Lấy dữ liệu với phân trang
            var items = dbSet
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
            using var context = _contextFactory.CreateDbContext(Array.Empty<string>());
            var dbSet = context.Set<TEntity>();
            return dbSet.Find(id);
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
            using var context = _contextFactory.CreateDbContext(Array.Empty<string>());
            var dbSet = context.Set<TEntity>();
            dbSet.Add(entity);
            context.SaveChanges();
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
            using var context = _contextFactory.CreateDbContext(Array.Empty<string>());
            var dbSet = context.Set<TEntity>();
            dbSet.Update(entity);
            context.SaveChanges();
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

            using var context = _contextFactory.CreateDbContext(Array.Empty<string>());
            var dbSet = context.Set<TEntity>();
            dbSet.Remove(entity);
            context.SaveChanges();
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
            using var context = _contextFactory.CreateDbContext(Array.Empty<string>());
            context.SaveChanges();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi lưu dữ liệu: {ex.Message}", ex);
        }
    }

    // ===== ASYNC Methods (modern, scalable) =====
    /// <summary>
    /// Lấy tất cả entities với phân trang (Async)
    /// </summary>
    public async Task<PagedResult<TEntity>> GetAllAsync(PagingRequest? paging = null)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext(Array.Empty<string>());
            var dbSet = context.Set<TEntity>();
            var result = new PagedResult<TEntity>();

            // Nếu không có phân trang, lấy tất cả
            if (paging == null)
            {
                result.Items = await dbSet.ToListAsync();
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
            var totalItems = await dbSet.CountAsync();
            var totalPages = (int)Math.Ceiling((decimal)totalItems / paging.PageSize);

            // Lấy dữ liệu với phân trang
            var items = await dbSet
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
    /// Thêm entity mới (Async)
    /// </summary>
    public async Task<TEntity> AddAsync(TEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        try
        {
            using var context = _contextFactory.CreateDbContext(Array.Empty<string>());
            var dbSet = context.Set<TEntity>();
            dbSet.Add(entity);

            // Use ExecutionStrategy to avoid Npgsql retry strategy conflict
            var strategy = context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () => await context.SaveChangesAsync());

            System.Diagnostics.Debug.WriteLine($"✅ [{typeof(TEntity).Name}] AddAsync completed successfully");
            return entity;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [{typeof(TEntity).Name}] AddAsync failed: {ex.GetType().Name}: {ex.Message}");
            if (ex.InnerException != null)
                System.Diagnostics.Debug.WriteLine($"   └─ Inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
            throw new InvalidOperationException($"Lỗi khi thêm {typeof(TEntity).Name}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Cập nhật entity (Async)
    /// </summary>
    public async Task UpdateAsync(TEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        try
        {
            using var context = _contextFactory.CreateDbContext(Array.Empty<string>());
            var dbSet = context.Set<TEntity>();
            dbSet.Update(entity);

            //  Use ExecutionStrategy to avoid Npgsql retry strategy conflict
            var strategy = context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () => await context.SaveChangesAsync());

            System.Diagnostics.Debug.WriteLine($"✅ [{typeof(TEntity).Name}] UpdateAsync completed successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [{typeof(TEntity).Name}] UpdateAsync failed: {ex.GetType().Name}: {ex.Message}");
            if (ex.InnerException != null)
                System.Diagnostics.Debug.WriteLine($"   └─ Inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
            throw new InvalidOperationException($"Lỗi khi cập nhật {typeof(TEntity).Name}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Xóa entity theo ID (Async)
    /// </summary>
    public async Task DeleteAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID phải lớn hơn 0", nameof(id));

        try
        {
            using var context = _contextFactory.CreateDbContext(Array.Empty<string>());
            var dbSet = context.Set<TEntity>();
            var entity = dbSet.Find(id);

            if (entity == null)
                throw new KeyNotFoundException($"{typeof(TEntity).Name} với ID {id} không tìm thấy");

            dbSet.Remove(entity);

            //  Use ExecutionStrategy to avoid Npgsql retry strategy conflict
            var strategy = context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () => await context.SaveChangesAsync());

            System.Diagnostics.Debug.WriteLine($"✅ [{typeof(TEntity).Name}] DeleteAsync(ID={id}) completed successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [{typeof(TEntity).Name}] DeleteAsync(ID={id}) failed: {ex.GetType().Name}: {ex.Message}");
            if (ex.InnerException != null)
                System.Diagnostics.Debug.WriteLine($"   └─ Inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
            throw new InvalidOperationException($"Lỗi khi xóa {typeof(TEntity).Name} với ID {id}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Lấy entities theo điều kiện với Include support (Async)
    /// </summary>
    public async Task<List<TEntity>> GetAsync(Expression<Func<TEntity, bool>>? predicate = null, params string[] includes)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext(Array.Empty<string>());
            var query = context.Set<TEntity>().AsQueryable();

            // Add Includes
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            // Apply predicate
            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return await query.ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi lấy dữ liệu {typeof(TEntity).Name}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Lưu tất cả thay đổi vào database (Async)
    /// </summary>
    public async Task SaveChangesAsync()
    {
        try
        {
            using var context = _contextFactory.CreateDbContext(Array.Empty<string>());
            // ✅ Use ExecutionStrategy to avoid Npgsql retry strategy conflict
            var strategy = context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () => await context.SaveChangesAsync());
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi lưu dữ liệu: {ex.Message}", ex);
        }
    }
}
