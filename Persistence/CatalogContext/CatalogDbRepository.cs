using Persistence.Common;

namespace Persistence.CatalogContext
{
    internal class CatalogDbRepository<T> : GenericRepository<T> where T : class
    {
        public CatalogDbRepository(ICatalogDbContext context) : base(context)
        {
        }
    }
}
