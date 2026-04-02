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

    public PagedResult<TEntity> GetAll(PagingRequest? paging = null)
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
            return result;
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

        return result;
    }

    public TEntity? GetById(int id)
    {
        return _data.FirstOrDefault(e => GetIdValue(e) == id);
    }

    public TEntity Add(TEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        // Tự động gán ID nếu ID hiện tại là 0
        var idProp = GetIdProperty();
        if (idProp != null && (int)idProp.GetValue(entity)! == 0)
        {
            idProp.SetValue(entity, _nextId++);
        }

        _data.Add(entity);
        return entity;
    }

    public void Update(TEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        var id = GetIdValue(entity);
        var existing = GetById(id);
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

    public void Delete(int id)
    {
        var entity = GetById(id);
        if (entity == null)
            throw new KeyNotFoundException($"{typeof(TEntity).Name} với ID {id} không tìm thấy");

        _data.Remove(entity);
    }

    public IQueryable<TEntity> Query()
    {
        return _data.AsQueryable();
    }

    public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
    {
        return _data.AsQueryable().Where(predicate).ToList();
    }

    public void SaveChanges()
    {
        // No-op for InMemory
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
