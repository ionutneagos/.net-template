using Domain.Entities.Tenant;
using Domain.Repositories;
using Services.Abstractions;
using Services.Abstractions.Tenant;

namespace Services.Tenant
{
    internal class SampleTenantService : GenericService<SampleTenant>, ISampleTenantService
    {
        public SampleTenantService(IServiceManager serviceManager, IGenericRepository<SampleTenant> repository) :
            base(serviceManager, repository)
        {

        }
    }
}
