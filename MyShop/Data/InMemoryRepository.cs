using System.Linq.Expressions;
using System.Reflection;

namespace MyShop.Data;

/// <summary>
/// Mock Repository implementation - lưu trữ dữ liệu trong bộ nhớ (In-memory)
/// Phù hợp cho testing, demo hoặc khi chưa có database
/// </summary>
/// <typeparam name="TEntity">Kiểu Entity</typeparam>
public class InMemoryRepository<TEntity> : IRepository<TEntity> where TEntity : class
{
    private static readonly List<TEntity> _data = new();
    private static int _nextId = 1;

    public Task<PagedResult<TEntity>> GetAll(PagingRequest? paging = null)
    {
        var result = new PagedResult<TEntity>();

        if (paging == null)
        {
            result.Items = _data.ToList();
            result.Pagination = new PagingMetadata
            {
                PageNumber = 1,
                PageSize = _data.Count,
                TotalItems = _data.Count
            };
            return Task.FromResult(result);
        }

        paging.Validate();
        var totalItems = _data.Count;
        var items = _data
            .Skip(paging.GetSkip())
            .Take(paging.PageSize)
            .ToList();

        result.Items = items;
        result.Pagination = new PagingMetadata
        {
            PageNumber = paging.PageNumber,
            PageSize = paging.PageSize,
            TotalItems = totalItems
        };

        return Task.FromResult(result);
    }

    public Task<TEntity?> GetById(int id)
    {
        var entity = _data.FirstOrDefault(e => GetIdValue(e) == id);
        return Task.FromResult(entity);
    }

    public Task<TEntity> Add(TEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        // Tự động gán ID nếu ID hiện tại là 0
        var idProp = GetIdProperty();
        if (idProp != null && (int)idProp.GetValue(entity)! == 0)
        {
            idProp.SetValue(entity, _nextId++);
        }

        _data.Add(entity);
        return Task.FromResult(entity);
    }

    public async Task Update(TEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        var id = GetIdValue(entity);
        var existing = await GetById(id);
        if (existing == null)
            throw new KeyNotFoundException($"{typeof(TEntity).Name} với ID {id} không tìm thấy");

        // Trong InMemory, object thường là cùng một reference, 
        // nhưng nếu khác reference ta thay thế nó
        if (!ReferenceEquals(existing, entity))
        {
            var index = _data.IndexOf(existing);
            _data[index] = entity;
        }
    }

    public async Task Delete(int id)
    {
        var entity = await GetById(id);
        if (entity == null)
            throw new KeyNotFoundException($"{typeof(TEntity).Name} với ID {id} không tìm thấy");

        _data.Remove(entity);
    }

    public IQueryable<TEntity> Query()
    {
        return _data.AsQueryable();
    }

    public Task<IEnumerable<TEntity>> Find(Expression<Func<TEntity, bool>> predicate)
    {
        IEnumerable<TEntity> results = _data.AsQueryable().Where(predicate).ToList();
        return Task.FromResult(results);
    }

    public Task SaveChanges()
    {
        // No-op for InMemory
        return Task.CompletedTask;
    }

    #region Helpers
    private PropertyInfo? GetIdProperty()
    {
        return typeof(TEntity).GetProperty("Id") ?? typeof(TEntity).GetProperty("id");
    }

    private int GetIdValue(TEntity entity)
    {
        var prop = GetIdProperty();
        if (prop == null) return 0;
        return (int)prop.GetValue(entity)!;
    }
    #endregion
}
