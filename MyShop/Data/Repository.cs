using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using MyShop.Models;
namespace MyShop.Data;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    private readonly AppDbContextFactory _contextFactory;

    public Repository(AppDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
    }


    public async Task<TResult> ExecuteAsync<TResult>(
        Func<AppDbContext, Task<TResult>> action,
        string operationName = "operation")
    {
        const int maxRetries = 3;
        int attempt = 0;

        while (attempt < maxRetries)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext(Array.Empty<string>());
                var strategy = context.Database.CreateExecutionStrategy();
                return await strategy.ExecuteAsync(async () => await action(context));
            }
            catch (Exception ex)
            {
                attempt++;

                // Kiểm tra lỗi có "retry được" không
                bool isRetryableError = ex.Message.Contains("ECIRCUITBREAKER") ||
                                       ex.Message.Contains("EMAXCONNSESSION") ||
                                       ex.Message.Contains("too many") ||
                                       ex.Message.Contains("authentication") ||
                                       ex.InnerException?.Message.Contains("ECIRCUITBREAKER") == true ||
                                       ex.InnerException?.Message.Contains("authentication") == true;

                // Nếu lỗi retry được VÀ còn attempt → chờ rồi retry
                if (isRetryableError && attempt < maxRetries)
                {
                    int delayMs = attempt * 1000; // 1s, 2s, 3s
                    System.Diagnostics.Debug.WriteLine(
                        $"⚠️ [{typeof(TEntity).Name}] {operationName} lỗi (attempt {attempt}/{maxRetries}). Chờ {delayMs}ms rồi retry...");
                    await Task.Delay(delayMs);
                    continue;
                }

                // Không phải retryable error hoặc hết attempts → throw
                System.Diagnostics.Debug.WriteLine($"❌ [{typeof(TEntity).Name}] {operationName} failed: {ex.GetType().Name}: {ex.Message}");
                if (ex.InnerException != null)
                    System.Diagnostics.Debug.WriteLine($"   └─ Inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
                throw new InvalidOperationException($"Lỗi khi thực hiện {operationName} trên {typeof(TEntity).Name}: {ex.Message}", ex);
            }
        }

        throw new InvalidOperationException($"Lỗi khi thực hiện {operationName} trên {typeof(TEntity).Name}: Exceeded max retries");
    }

    /// <summary>
    /// Gom GetAllAsync lại - gọi GetAllAsyncWithIncludes với includes trống cho gọn
    /// </summary>
    public async Task<PagedResult<TEntity>> GetAllAsync(
        PagingRequest? paging = null,
        Expression<Func<TEntity, bool>>? filter = null,
        params string[] includes)
    {
        return await GetAllAsyncWithIncludes(paging, filter, includes);
    }

    /// <summary>
    /// Thêm entity mới - Trả về entity có kèm ID mới từ database (Async)
    /// </summary>
    public async Task<TEntity> AddAsync(TEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var result = await ExecuteAsync(async context =>
        {
            context.Add(entity);
            await context.SaveChangesAsync();
            return entity; // Trả về entity để có được ID mới
        }, "AddAsync");

        System.Diagnostics.Debug.WriteLine($" [{typeof(TEntity).Name}] AddAsync completed successfully");
        return result;
    }

    /// <summary>
    /// Cập nhật entity (Async)
    /// </summary>
    public async Task<TEntity> UpdateAsync(TEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var result = await ExecuteAsync(async context =>
        {
            context.Update(entity);
            await context.SaveChangesAsync();
            return entity;
        }, "UpdateAsync");

        System.Diagnostics.Debug.WriteLine($" [{typeof(TEntity).Name}] UpdateAsync completed successfully");
        return result;
    }
    public async Task DeleteAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID phải lớn hơn 0", nameof(id));

        await ExecuteAsync(async context =>
        {
            var entity = await context.Set<TEntity>().FindAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"{typeof(TEntity).Name} với ID {id} không tìm thấy");

            context.Remove(entity);
            await context.SaveChangesAsync();
            return true;
        }, $"DeleteAsync(ID={id})");

        System.Diagnostics.Debug.WriteLine($" [{typeof(TEntity).Name}] DeleteAsync(ID={id}) completed successfully");
    }

    public async Task<TEntity?> GetByIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID phải lớn hơn 0", nameof(id));

        return await ExecuteAsync(async context =>
        {
            return await context.Set<TEntity>().FindAsync(id);
        }, $"GetByIdAsync(ID={id})");
    }

    public async Task<List<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        params string[] includes)
    {
        return await ExecuteAsync(async context =>
        {
            var query = context.Set<TEntity>().AsQueryable();

            // Add Includes
            foreach (var include in includes)
                query = query.Include(include);

            // Apply predicate
            if (predicate != null)
                query = query.Where(predicate);

            return await query.ToListAsync();
        }, "GetAsync");
    }

    /// <summary>
    /// Kiểm tra entity có tồn tại với điều kiện (Async)
    /// </summary>
    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await ExecuteAsync(async context =>
        {
            return await context.Set<TEntity>().AnyAsync(predicate);
        }, "AnyAsync");
    }

    public async Task<PagedResult<TEntity>> GetAllAsyncWithIncludes(
        PagingRequest? paging = null,
        Expression<Func<TEntity, bool>>? filter = null,
        params string[] includes)
    {
        return await ExecuteAsync(async context =>
        {
            var query = context.Set<TEntity>().AsNoTracking();

            // Apply Includes FIRST (before filter for better performance)
            foreach (var include in includes)
                query = query.Include(include);

            // Apply filter if available
            if (filter != null)
                query = query.Where(filter);

            // If no pagination, get all
            if (paging == null)
            {
                var allItems = await query.ToListAsync();
                return new PagedResult<TEntity>
                {
                    Items = allItems,
                    Pagination = new PagingMetadata
                    {
                        PageNumber = 1,
                        PageSize = allItems.Count,
                        TotalItems = allItems.Count,
                    }
                };
            }

            // Validate pagination
            paging.Validate();

            // Count total items (MUST be after filter is applied)
            var totalItems = await query.CountAsync();

            // Get items with Skip/Take
            var items = await query
                .Skip(paging.GetSkip())
                .Take(paging.PageSize)
                .ToListAsync();

            return new PagedResult<TEntity>
            {
                Items = items,
                Pagination = new PagingMetadata
                {
                    PageNumber = paging.PageNumber,
                    PageSize = paging.PageSize,
                    TotalItems = totalItems,
                }
            };
        }, "GetAllAsyncWithIncludes");
    }

    /// <summary>
    /// Đếm entities với filter (Async)
    /// </summary>
    public async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null)
    {
        return await ExecuteAsync(async context =>
        {
            var query = context.Set<TEntity>().AsQueryable();
            if (predicate != null)
                query = query.Where(predicate);
            return await query.CountAsync();
        }, "CountAsync");
    }

    /// <summary>
    /// Lưu tất cả thay đổi vào database (Async)
    /// </summary>
    public async Task SaveChangesAsync()
    {
        await ExecuteAsync(async context =>
        {
            await context.SaveChangesAsync();
            return true;
        }, "SaveChangesAsync");
    }
}
