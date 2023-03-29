using Contracts.Catalog.AppTenant.Request;
using Contracts.Catalog.AppTenant.Response;
using Domain.Entities.Catalog;
using Domain.Repositories;
using Mapster;
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

        public async Task<TenantResponse> AddAsync(CreateTenantRequest request, CancellationToken cancellationToken = default)
        {
            AppTenant entity = request.Adapt<AppTenant>();
            entity.NormalizedName = entity.Name.Normalize().ToUpperInvariant();
            await CreateAsync(entity, cancellationToken);
            return entity.Adapt<TenantResponse>();
        }
    }
}
