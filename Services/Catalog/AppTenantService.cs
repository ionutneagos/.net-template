using Domain.Entities.Catalog;
using Domain.Repositories;
using Services.Abstractions;
using Services.Abstractions.Catalog;

namespace Services.Catalog
{
    internal sealed class AppTenantService : GenericService<AppTenant>, IAppTenantService
    {
        public AppTenantService(IServiceManager serviceManager, IGenericRepository<AppTenant> repository)
            : base(serviceManager, repository)
        {
        }

    }
}
