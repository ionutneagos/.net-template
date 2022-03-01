using Microsoft.Extensions.DependencyInjection;

namespace Persistence.TenantContext
{
    internal static class SetupTenantContext
    {
        public static void AddTenantContext(this IServiceCollection services)
        {
            services.AddDbContext<TenantDbContext>(ServiceLifetime.Scoped);
        }
    }
}