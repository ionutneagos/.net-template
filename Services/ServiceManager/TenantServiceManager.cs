using Domain.Constants;
using Domain.Entities.Tenant;
using Services.Abstractions.Tenant;
using Services.Tenant;

namespace Services
{
    public partial class ServiceManager
    {
        private Lazy<ISampleTenantService> _lazySampleTenantService;

        private static string TenantContext => ContextConfiguration.TenantContextName;

        private void InitTenantServices()
        {
            _lazySampleTenantService = new Lazy<ISampleTenantService>(() => new SampleTenantService(this, RepositoryResolver<SampleTenant>(TenantContext)));
        }

        public ISampleTenantService SampleTenantService => _lazySampleTenantService.Value;

    }
}