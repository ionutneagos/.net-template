using Contracts.Catalog.AppTenant.Request;
using Contracts.Catalog.AppTenant.Response;
using Domain.Entities.Catalog;

namespace Services.Abstractions.Catalog
{
    public interface IAppTenantService : IGenericService<AppTenant>
    {
        Task<TenantResponse> AddAsync(CreateTenantRequest request, CancellationToken cancellationToken = default);
    }
}
