using Domain.Constants;
using Domain.Entities.Catalog;
using Services.Abstractions.Catalog;
using Services.Catalog;

namespace Services
{
    public partial class ServiceManager
    {
        private Lazy<ISampleService> _lazySampleService;
        private Lazy<IAppTenantService> _lazyTenantService;
        private Lazy<IAppUserService> _lazyAppUserService;

        private static string CatalogContext => ContextConfiguration.CatalogContextName;

        private void InitCatalogServices()
        {
            _lazySampleService = new Lazy<ISampleService>(() => new SampleService(this, RepositoryResolver<Sample>(CatalogContext)));
            _lazyTenantService = new Lazy<IAppTenantService>(() => new AppTenantService(this, RepositoryResolver<AppTenant>(CatalogContext)));
            _lazyAppUserService = new Lazy<IAppUserService>(() => new AppUserService(this, RepositoryResolver<ApplicationUser>(CatalogContext)));
        }

        public ISampleService SampleService => _lazySampleService.Value;
        public IAppTenantService AppTenantService => _lazyTenantService.Value;
        public IAppUserService AppUserService => _lazyAppUserService.Value;
    }
}