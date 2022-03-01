using Microsoft.EntityFrameworkCore;

namespace Domain.Repositories
{
    public interface IRepositoryContext : IDisposable
    {
        DbSet<T> Set<T>() where T : class;
        string Provider { get; }
        string ContextName { get; }
        void Commit(dynamic? userId);
        Task CommitAsync(dynamic? userId, CancellationToken cancellationToken = default);
        void Rollback();
    }
}
