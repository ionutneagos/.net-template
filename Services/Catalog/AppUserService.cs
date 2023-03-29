using Domain.Constants;
using Domain.Entities.Catalog;
using Domain.Extensions;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
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

        public override IQueryable<ApplicationUser> GetAll()
        {
            if (serviceManager.User.IsInRole(IdentityConfiguration.RootRole))
                return base.GetAll();
            else
            {
                int? tenantId = serviceManager.User.GetTenantFromClaim();
                return base.GetAll().Where(t => t.TenantId == tenantId).Include(t => t.Tenant);
            }
        }
    }
}