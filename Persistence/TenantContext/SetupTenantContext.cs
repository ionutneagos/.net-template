using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Persistence.TenantContext
{
    internal static class SetupTenantContext
    {
        public static void AddTenantContext(this IServiceCollection services, IConfiguration configuration)
        {
            string? cs = configuration["Database:TenantConnectionString"];
            services.AddDbContext<TenantDbContext>(options =>
            {
                options.UseSqlServer(cs, providerOptions =>
                {
                    providerOptions.EnableRetryOnFailure();
                });
            }, ServiceLifetime.Scoped
            );
        }
    }
}