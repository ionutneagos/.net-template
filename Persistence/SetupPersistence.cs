using Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.CatalogContext;
using Persistence.TenantContext;

namespace Persistence
{
    public static class SetupPersistence
    {
        public static void AddPersistenceContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCatalogContext(configuration);

            services.AddScoped<ICatalogDbContext, CatalogDbContext>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(CatalogDbRepository<>));

            services.AddTenantContext(configuration);
            services.AddScoped<ITenantDbContext, TenantDbContext>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(TenantDbRepository<>));
        }
    }
}