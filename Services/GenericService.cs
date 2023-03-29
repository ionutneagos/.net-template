using Domain.Repositories;
using Services.Abstractions;
using System.Linq.Expressions;

namespace Services
{
    internal class GenericService<T> : BaseService<T>, IGenericService<T> where T : class
    {
        public GenericService(IServiceManager serviceManager, IGenericRepository<T> repository)
            : base(serviceManager, repository)
        {
        }

        public virtual IQueryable<T> GetAll()
        {
            return repository.GetAll();
        }
        public virtual IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression)
        {
            return repository.FindByCondition(expression);
        }

        public virtual async Task<IEnumerable<T>> Get(Expression<Func<T, bool>> filter, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string includeProperties = "", CancellationToken cancellationToken = default)
        {
            return await repository.GetAsync(filter, orderBy, includeProperties, cancellationToken);
        }

        public virtual async Task<T> GetByIdAsync(object id)
        {
            return await repository.GetByIdAsync(id);
        }

        public virtual async Task<T> GetByIdAsync(object[] values, CancellationToken cancellationToken = default)
        {
            return await repository.GetByIdAsync(values, cancellationToken);
        }

        public virtual void Add(T entity)
        {
            repository.Create(entity);
        }

        public virtual void Create(T entity)
        {
            repository.Create(entity);
            serviceManager.Commit(repository.Context.ContextName);
        }

        public virtual async Task CreateAsync(T entity, CancellationToken cancellationToken = default)
        {
            repository.Create(entity);
            await serviceManager.CommitAsync(repository.Context.ContextName, cancellationToken);
        }

        public virtual async Task CreateRangeAsync(List<T> entities, CancellationToken cancellationToken = default)
        {
            await repository.CreateRangeAsync(entities, cancellationToken);
            await serviceManager.CommitAsync(repository.Context.ContextName, cancellationToken);
        }

        public void Update(T entity)
        {
            repository.Update(entity);
            serviceManager.Commit(repository.Context.ContextName);
        }
        public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            repository.Update(entity);
            await serviceManager.CommitAsync(repository.Context.ContextName, cancellationToken);
        }

        public async Task UpdateRangeAsync(List<T> entities, CancellationToken cancellationToken = default)
        {
            repository.UpdateRange(entities);
            await serviceManager.CommitAsync(repository.Context.ContextName, cancellationToken);
        }
        public void Delete(object id)
        {
            repository.Delete(id);
            serviceManager.Commit(repository.Context.ContextName);
        }
        public async Task DeleteAsync(object id, CancellationToken cancellationToken = default)
        {
            repository.Delete(id);
            await serviceManager.CommitAsync(repository.Context.ContextName, cancellationToken);
        }
        public virtual void Delete(T entityToDelete)
        {
            repository.Delete(entityToDelete);
            serviceManager.Commit(repository.Context.ContextName);
        }
        public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            repository.Delete(entity);
            await serviceManager.CommitAsync(repository.Context.ContextName, cancellationToken);
        }

        public virtual void DeleteRange(List<T> entities)
        {
            repository.DeleteRange(entities);
            serviceManager.Commit(repository.Context.ContextName);
        }

        public async Task DeleteRangeAsync(List<T> entities, CancellationToken cancellationToken = default)
        {
            repository.DeleteRange(entities);
            await serviceManager.CommitAsync(repository.Context.ContextName, cancellationToken);
        }

        public virtual void RemoveRange(List<T> entities)
        {
            repository.DeleteRange(entities);
        }

        #region Dispose Methods
        private bool disposed = false;
        public void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    serviceManager.Dispose();
                }
            }
            disposed = true;
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}