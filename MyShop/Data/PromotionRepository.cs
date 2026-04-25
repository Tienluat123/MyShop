using Microsoft.EntityFrameworkCore;
using MyShop.Models;

namespace MyShop.Data;

/// <summary>
/// Repository dành riêng cho Promotion.
/// KHÔNG kế thừa Repository (vì các method không có virtual).
/// Thay vào đó chứa các method "Fresh" để LUON đọc từ database.
///
/// Vì đây là file riêng của Promotion, các thay đổi không ảnh hưởng đến
/// các entity khác (Product, Customer, Order...) của các thành viên khác trong nhóm.
///
/// Cơ chế: GetAllFresh() và GetByIdFresh() luôn clear ChangeTracker trước khi query
/// để EF Core không trả về entity đã được track với giá trị cũ (stale data).
/// </summary>
public class PromotionRepository : IPromotionRepository
{
    private readonly AppDbContext _context;
    private readonly DbSet<Promotion> _dbSet;

    public PromotionRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<Promotion>();
    }

    /// <summary>
    /// Lấy tất cả promotions — LUON từ database, không dùng tracked entities.
    /// Fix stale data bug: khi có Add/Update/Delete trước đó, EF Core ChangeTracker
    /// có thể trả về entity cũ thay vì query mới từ DB.
    /// </summary>
    public async Task<PagedResult<Promotion>> GetAllFreshAsync(
        PagingRequest? paging = null, 
        System.Linq.Expressions.Expression<Func<Promotion, bool>>? filter = null,
        CancellationToken ct = default)
    {
        var result = new PagedResult<Promotion>();

        // LUON clear ChangeTracker de khong co stale entity
        _context.ChangeTracker.Clear();

        var query = _dbSet.AsNoTracking();
        if (filter != null)
        {
            query = query.Where(filter);
        }

        if (paging == null)
        {
            result.Items = await query.ToListAsync(ct);
            result.Pagination = new PagingMetadata
            {
                PageNumber = 1,
                PageSize = result.Items.Count,
                TotalItems = result.Items.Count,
            };
            return result;
        }

        paging.Validate();
        var totalItems = await query.CountAsync(ct);

        // Apply sorting based on SortColumn and SortAscending
        var sortedQuery = paging.SortColumn switch
        {
            "Code" => paging.SortAscending
                ? query.OrderBy(p => p.PromoCode)
                : query.OrderByDescending(p => p.PromoCode),

            "Type" => paging.SortAscending
                ? query.OrderBy(p => p.PromoType)
                : query.OrderByDescending(p => p.PromoType),

            "Value" => paging.SortAscending
                ? query.OrderBy(p => p.DiscountValue)
                : query.OrderByDescending(p => p.DiscountValue),

            "Status" => paging.SortAscending
                ? query.OrderBy(p => p.IsActive)
                : query.OrderByDescending(p => p.IsActive),

            "MinOrder" => paging.SortAscending
                ? query.OrderBy(p => p.MinOrderAmount)
                : query.OrderByDescending(p => p.MinOrderAmount),

            "StartDate" => paging.SortAscending
                ? query.OrderBy(p => p.StartDate)
                : query.OrderByDescending(p => p.StartDate),

            "Target" => paging.SortAscending
                ? query.OrderBy(p => p.TargetAudience)
                : query.OrderByDescending(p => p.TargetAudience),

            // Mặc định sort theo PromoCode tăng dần
            _ => query.OrderBy(p => p.PromoCode)
        };

        var items = await sortedQuery
            .Skip(paging.GetSkip())
            .Take(paging.PageSize)
            .ToListAsync(ct);

        result.Items = items;
        result.Pagination = new PagingMetadata
        {
            PageNumber = paging.PageNumber,
            PageSize = paging.PageSize,
            TotalItems = totalItems,
        };
        return result;
    }

    /// <summary>
    /// Lấy promotion theo ID — LUON từ database.
    /// Find() trả về tracked entity, nếu entity đó đã bị sửa mà chưa save thì
    /// sẽ trả về giá trị cũ. Clear tracker trước để đảm bảo lấy từ DB.
    /// </summary>
    public async Task<Promotion?> GetByIdFreshAsync(int id, CancellationToken ct = default)
    {
        if (id <= 0) return null;
        _context.ChangeTracker.Clear();
        return await _dbSet.FindAsync(new object[] { id }, ct);
    }

    /// <summary>
    /// Thêm promotion mới.
    /// </summary>
    public async Task<Promotion> AddAsync(Promotion entity, CancellationToken ct = default)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _dbSet.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
        return entity;
    }

    /// <summary>
    /// Cập nhật promotion.
    /// </summary>
    public async Task UpdateAsync(Promotion entity, CancellationToken ct = default)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        _dbSet.Update(entity);
        await _context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Xóa promotion theo ID.
    /// </summary>
    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        if (id <= 0) throw new ArgumentException("ID phải lớn hơn 0", nameof(id));
        var entity = await _dbSet.FindAsync(new object[] { id }, ct);
        if (entity == null) throw new KeyNotFoundException($"Promotion với ID {id} không tìm thấy");
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }
}
