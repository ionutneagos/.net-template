using Persistence.Common;

namespace Persistence.TenantContext
{
    internal class TenantDbRepository<T> : GenericRepository<T> where T : class
    {
        public TenantDbRepository(ITenantDbContext context) : base(context)
        {

        }
    }
}
