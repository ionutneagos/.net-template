using System.Linq.Expressions;

namespace Domain.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        IRepositoryContext Context { get; }
        IQueryable<T> GetAll();
        IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression);

        Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> filter, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
             string includeProperties = "", CancellationToken cancellationToken = default);

        Task<T> GetByIdAsync(object id, CancellationToken cancellationToken = default);
        Task<T> GetByIdAsync(object[] values, CancellationToken cancellationToken = default);

        void Create(T entity);

        Task CreateRangeAsync(List<T> entities, CancellationToken cancellationToken = default);

        void Update(T entity);

        void UpdateRange(List<T> entities);

        void Delete(object id);
        void Delete(T entity);

        void DeleteRange(List<T> entities);
    }
}
