using System.Linq.Expressions;
namespace MyShop.Data;

/// <summary>
/// Generic repository interface định nghĩa các CRUD operations cơ bản
/// Tất cả repositories phải implement interface này
/// </summary>
/// <typeparam name="TEntity">Kiểu Entity (Model)</typeparam>
public interface IRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Lấy tất cả entities với phân trang
    /// </summary>
    /// <param name="paging">Phân trang (nếu không cung cấp lấy hết)</param>
    /// <returns>PagedResult chứa danh sách entities + metadata</returns>
    PagedResult<TEntity> GetAll(PagingRequest? paging = null);

    /// <summary>
    /// Lấy entity theo ID
    /// </summary>
    /// <param name="id">ID của entity</param>
    /// <returns>Entity hoặc null nếu không tìm thấy</returns>
    TEntity? GetById(int id);

    /// <summary>
    /// Thêm entity mới
    /// </summary>
    /// <param name="entity">Entity cần thêm</param>
    /// <returns>Entity sau khi được thêm (có ID đã gán)</returns>
    TEntity Add(TEntity entity);

    /// <summary>
    /// Cập nhật entity
    /// </summary>
    /// <param name="entity">Entity cần cập nhật</param>
    void Update(TEntity entity);

    /// <summary>
    /// Xóa entity theo ID
    /// </summary>
    /// <param name="id">ID của entity cần xóa</param>
    void Delete(int id);

    /// <summary>
    /// Lấy IQueryable để thực hiện LINQ queries phức tạp
    /// </summary>
    IQueryable<TEntity> Query();

    /// <summary>
    /// Tìm kiếm entities theo điều kiện
    /// </summary>
    IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Lưu tất cả thay đổi vào database
    /// </summary>
    void SaveChanges();
}
