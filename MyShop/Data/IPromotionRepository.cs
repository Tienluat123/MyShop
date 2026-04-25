using MyShop.Models;

namespace MyShop.Data;

/// <summary>
/// Interface cho PromotionRepository — tách biệt với IRepository chung của cả nhóm.
/// Chỉ chứa method "Fresh" để đảm bảo luôn đọc từ database.
/// </summary>
public interface IPromotionRepository
{
    /// <summary>
    /// Lấy tất cả promotions — luôn từ database, không dùng EF cached entities.
    /// </summary>
    Task<PagedResult<Promotion>> GetAllFreshAsync(
        PagingRequest? paging = null, 
        System.Linq.Expressions.Expression<Func<Promotion, bool>>? filter = null,
        CancellationToken ct = default);

    /// <summary>
    /// Lấy promotion theo ID — luôn từ database.
    /// </summary>
    Task<Promotion?> GetByIdFreshAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Thêm promotion mới.
    /// </summary>
    Task<Promotion> AddAsync(Promotion entity, CancellationToken ct = default);

    /// <summary>
    /// Cập nhật promotion.
    /// </summary>
    Task UpdateAsync(Promotion entity, CancellationToken ct = default);

    /// <summary>
    /// Xóa promotion theo ID.
    /// </summary>
    Task DeleteAsync(int id, CancellationToken ct = default);
}
