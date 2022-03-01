using Domain.Entities.Catalog;
using Domain.Repositories;
using Services.Abstractions;
using Services.Abstractions.Catalog;

namespace Services.Catalog
{
    internal sealed class AppUserService : GenericService<ApplicationUser>, IAppUserService
    {
        public AppUserService(IServiceManager serviceManager, IGenericRepository<ApplicationUser> repository)
            : base(serviceManager, repository)
        {
        }
    }
}