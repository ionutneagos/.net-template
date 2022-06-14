using System.Linq.Expressions;

namespace Services.Abstractions
{
    public interface IGenericService<T> : IDisposable
    {
        IQueryable<T> GetAll();
        IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression);

        Task<IEnumerable<T>> Get(Expression<Func<T, bool>> filter, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
              string includeProperties = "", CancellationToken cancellationToken = default);

        Task<T> GetByIdAsync(object id);
        Task<T> GetByIdAsync(object[] values, CancellationToken cancellationToken = default);

        void Add(T entity);
        void Create(T entity);
        Task CreateRangeAsync(List<T> entities, CancellationToken cancellationToken = default);

        Task CreateAsync(T entity, CancellationToken cancellationToken = default);

        void Update(T entity);
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

        Task UpdateRangeAsync(List<T> entities, CancellationToken cancellationToken = default);

        void Delete(object id);
        Task DeleteAsync(object id, CancellationToken cancellationToken = default);

        void Delete(T entityToDelete);
        Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

        void DeleteRange(List<T> entities);
        Task DeleteRangeAsync(List<T> entities, CancellationToken cancellationToken = default);

        void RemoveRange(List<T> entities);
    }
}
