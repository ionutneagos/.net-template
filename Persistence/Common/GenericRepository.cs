using Domain.Exceptions;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

#nullable disable
namespace Persistence.Common
{
    public abstract class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        #region Variables
        private readonly DbContext context;
        private readonly DbSet<T> DbSet;
        #endregion

        IRepositoryContext IGenericRepository<T>.Context => (IRepositoryContext)context;
        public GenericRepository(IRepositoryContext context)
        {
            this.context = context as DbContext;
            DbSet = this.context.Set<T>();
        }

        public IQueryable<T> GetAll()
        {
            return DbSet.AsNoTracking();
        }

        public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression)
        {
            return DbSet.Where(expression).AsNoTracking();
        }

        public virtual async Task<IEnumerable<T>> Get(Expression<Func<T, bool>> filter, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = "", CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = DbSet;

            if (filter != null)
                query = query.Where(filter);

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                return await orderBy(query).ToListAsync(cancellationToken);
            }
            else
            {
                return await query.ToListAsync(cancellationToken);
            }
        }

        public virtual async Task<T> GetByIdAsync(object id)
        {
            var result = await DbSet.FindAsync(id);
            if (result == null)
                throw new NotFoundException("Entity not found!");
            return result;
        }

        public virtual async Task<T> GetByIdAsync(object[] values, CancellationToken cancellationToken = default)
        {
            var result = await DbSet.FindAsync(values, cancellationToken);
            if (result == null)
                throw new NotFoundException("Entity not found!");
            return result;
        }

        public virtual void Create(T entity)
        {
            DbSet.Add(entity);
        }

        public virtual async Task CreateRangeAsync(List<T> entities, CancellationToken cancellationToken = default)
        {
            await DbSet.AddRangeAsync(entities, cancellationToken);
        }

        public virtual void Update(T entity)
        {
            DbSet.Attach(entity);
            context.Entry(entity).State = EntityState.Modified;
        }

        public virtual void UpdateRange(List<T> entities)
        {
            DbSet.UpdateRange(entities);
        }
        public virtual void Delete(object id)
        {
            T entityToDelete = DbSet.Find(id);

            if (entityToDelete != null)
                Delete(entityToDelete);

        }
        public virtual void DeleteRange(List<T> entities)
        {
            DbSet.RemoveRange(entities);
        }
        public virtual void Delete(T entityToDelete)
        {
            if (context.Entry(entityToDelete).State == EntityState.Detached)
            {
                DbSet.Attach(entityToDelete);
            }
            DbSet.Remove(entityToDelete);
        }
    }
}
