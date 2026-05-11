using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using MyShop.Models;

namespace MyShop.Data;

public class PromotionRepository : IPromotionRepository
{
    private readonly AppDbContextFactory _contextFactory;

    public PromotionRepository(AppDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
    }

    private AppDbContext CreateContext() => _contextFactory.CreateDbContext(Array.Empty<string>());

    private async Task<TResult> ExecuteWithThrottleAsync<TResult>(Func<AppDbContext, Task<TResult>> action)
    {
        await DbThrottle.GlobalSemaphore.WaitAsync();
        try
        {
            using var context = CreateContext();
            return await action(context);
        }
        finally
        {
            DbThrottle.GlobalSemaphore.Release();
        }
    }

    public async Task<PagedResult<Promotion>> GetAllFreshAsync(
        PagingRequest? paging = null, 
        System.Linq.Expressions.Expression<Func<Promotion, bool>>? filter = null,
        CancellationToken ct = default)
    {
        return await ExecuteWithThrottleAsync(async context => 
        {
            var result = new PagedResult<Promotion>();
            var query = context.Set<Promotion>().AsNoTracking();
            if (filter != null) query = query.Where(filter);

            if (paging == null)
            {
                result.Items = await query.ToListAsync(ct);
                result.Pagination = new PagingMetadata { TotalItems = result.Items.Count };
                return result;
            }

            paging.Validate();
            var totalItems = await query.CountAsync(ct);
            IQueryable<Promotion> sortedQuery = !string.IsNullOrWhiteSpace(paging.SortBy)
                ? query.OrderBy(paging.SortBy)
                : query.OrderBy(p => p.PromoCode);

            result.Items = await sortedQuery.Skip(paging.GetSkip()).Take(paging.PageSize).ToListAsync(ct);
            result.Pagination = new PagingMetadata { PageNumber = paging.PageNumber, PageSize = paging.PageSize, TotalItems = totalItems };
            return result;
        });
    }

    public async Task<Promotion?> GetByIdFreshAsync(int id, CancellationToken ct = default)
    {
        return await ExecuteWithThrottleAsync(async context => 
        {
            return await context.Set<Promotion>().AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        });
    }

    public async Task<Promotion> AddAsync(Promotion entity, CancellationToken ct = default)
    {
        return await ExecuteWithThrottleAsync(async context => 
        {
            await context.Set<Promotion>().AddAsync(entity, ct);
            await context.SaveChangesAsync(ct);
            return entity;
        });
    }

    public async Task UpdateAsync(Promotion entity, CancellationToken ct = default)
    {
        await ExecuteWithThrottleAsync(async context => 
        {
            context.Set<Promotion>().Update(entity);
            await context.SaveChangesAsync(ct);
            return true;
        });
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await ExecuteWithThrottleAsync(async context => 
        {
            var entity = await context.Set<Promotion>().FindAsync(new object[] { id }, ct);
            if (entity != null)
            {
                context.Set<Promotion>().Remove(entity);
                await context.SaveChangesAsync(ct);
            }
            return true;
        });
    }
}
