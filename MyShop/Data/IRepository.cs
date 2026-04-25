namespace MyShop.Data;

using System.Linq.Expressions;


public interface IRepository<TEntity> where TEntity : class
{

    /// <summary>
    /// Lấy tất cả entities với phân trang, Include support, và filter tùy chỉnh (Async)
    /// 
    /// Gịm GetAllAsync + GetAllAsyncWithIncludes chó gọn
    /// </summary>
    Task<PagedResult<TEntity>> GetAllAsync(
        PagingRequest? paging = null,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        params string[] includes);

    Task<TEntity?> GetByIdAsync(int id);

    Task<List<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        params string[] includes);

    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);
    Task<TResult> ExecuteAsync<TResult>(
        Func<AppDbContext, Task<TResult>> action,
        string operationName = "operation");

    Task<PagedResult<TEntity>> GetAllAsyncWithIncludes(
        PagingRequest? paging = null,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        params string[] includes);

    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null);
    /// <summary>
    /// Thêm entity mới - Trả về entity có kèm ID mới từ database (Async)
    /// </summary>
    Task<TEntity> AddAsync(TEntity entity);

    /// <summary>
    /// Cập nhật entity (Async)
    /// </summary>
    Task<TEntity> UpdateAsync(TEntity entity);

    Task DeleteAsync(int id);

    Task SaveChangesAsync();
}
