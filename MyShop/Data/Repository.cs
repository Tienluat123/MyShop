using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Linq.Dynamic.Core;
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
        const int maxRetries = 10;
        int attempt = 0;

        while (attempt < maxRetries)
        {
            await DbThrottle.GlobalSemaphore.WaitAsync();
            try
            {
                using var context = _contextFactory.CreateDbContext(Array.Empty<string>());
                var strategy = context.Database.CreateExecutionStrategy();
                return await strategy.ExecuteAsync(async () => await action(context));
            }
            catch (Exception ex)
            {
                attempt++;

                bool isRetryableError = ex.Message.Contains("ECIRCUITBREAKER") ||
                                       ex.Message.Contains("EMAXCONNSESSION") ||
                                       ex.Message.Contains("too many") ||
                                       ex.Message.Contains("authentication") ||
                                       ex.Message.Contains("max clients") ||
                                       ex.InnerException?.Message.Contains("ECIRCUITBREAKER") == true ||
                                       ex.InnerException?.Message.Contains("EMAXCONNSESSION") == true ||
                                       ex.InnerException?.Message.Contains("max clients") == true;

                if (isRetryableError && attempt < maxRetries)
                {
                    int delayMs = attempt * 1000; 
                    System.Diagnostics.Debug.WriteLine($"⚠️ [RETRY] {operationName} (Attempt {attempt}/{maxRetries}). Wait {delayMs}ms...");
                    await Task.Delay(delayMs);
                    continue;
                }

                System.Diagnostics.Debug.WriteLine($"❌ [FATAL] {operationName} failed: {ex.Message}");
                throw new InvalidOperationException($"Database error in {operationName}: {ex.Message}", ex);
            }
            finally
            {
                DbThrottle.GlobalSemaphore.Release();
            }
        }

        throw new InvalidOperationException($"Database busy after {maxRetries} retries in {operationName}");
    }

    public async Task<TEntity?> GetByIdAsync(int id)
    {
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
            var query = context.Set<TEntity>().AsNoTracking();

            foreach (var include in includes)
                query = query.Include(include);

            if (predicate != null)
                query = query.Where(predicate);

            return await query.ToListAsync();
        }, "GetAsync");
    }

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await ExecuteAsync(async context =>
        {
            return await context.Set<TEntity>().AnyAsync(predicate);
        }, "AnyAsync");
    }

    public async Task<PagedResult<TEntity>> GetAllAsync(
        PagingRequest? paging = null,
        Expression<Func<TEntity, bool>>? filter = null,
        params string[] includes)
    {
        return await ExecuteAsync(async context =>
        {
            var result = new PagedResult<TEntity>();
            var query = context.Set<TEntity>().AsNoTracking();

            foreach (var include in includes)
                query = query.Include(include);

            if (filter != null)
                query = query.Where(filter);

            if (paging == null)
            {
                result.Items = await query.ToListAsync();
                result.Pagination = new PagingMetadata
                {
                    PageNumber = 1,
                    PageSize = result.Items.Count,
                    TotalItems = result.Items.Count
                };
                return result;
            }

            paging.Validate();
            var totalItems = await query.CountAsync();

            var items = await query
                .Skip(paging.GetSkip())
                .Take(paging.PageSize)
                .ToListAsync();

            result.Items = items;
            result.Pagination = new PagingMetadata
            {
                PageNumber = paging.PageNumber,
                PageSize = paging.PageSize,
                TotalItems = totalItems
            };
            return result;
        }, "GetAllAsync");
    }

    public async Task<TEntity> AddAsync(TEntity entity)
    {
        return await ExecuteAsync(async context =>
        {
            await context.Set<TEntity>().AddAsync(entity);
            await context.SaveChangesAsync();
            return entity;
        }, "AddAsync");
    }

    public async Task<TEntity> UpdateAsync(TEntity entity)
    {
        return await ExecuteAsync(async context =>
        {
            context.Set<TEntity>().Update(entity);
            await context.SaveChangesAsync();
            return entity;
        }, "UpdateAsync");
    }

    public async Task DeleteAsync(int id)
    {
        await ExecuteAsync(async context =>
        {
            var entity = await context.Set<TEntity>().FindAsync(id);
            if (entity != null)
            {
                context.Set<TEntity>().Remove(entity);
                await context.SaveChangesAsync();
            }
            return true;
        }, "DeleteAsync");
    }

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>>? filter = null)
    {
        return await ExecuteAsync(async context =>
        {
            var query = context.Set<TEntity>().AsNoTracking();
            if (filter != null)
                query = query.Where(filter);
            return await query.CountAsync();
        }, "CountAsync");
    }

    public async Task SaveChangesAsync()
    {
        await Task.CompletedTask;
    }

    public async Task<PagedResult<TEntity>> GetAllAsyncWithIncludes(
        PagingRequest? paging = null,
        Expression<Func<TEntity, bool>>? filter = null,
        params string[] includes)
    {
        return await GetAllAsync(paging, filter, includes);
    }
}
