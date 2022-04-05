using EFCore.AutomaticMigrations;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.CatalogContext.CatalogSeed;

namespace Persistence.CatalogContext
{
    public static class SetupCatalogContext
    {
        public static void AddCatalogContext(this IServiceCollection services, IConfiguration configuration)
        {
            var cs = configuration["Database:CatalogConnectionString"];
            services.AddDbContext<CatalogDbContext>(options =>
            {
                options.UseSqlServer(cs, providerOptions =>
                {
                    providerOptions.EnableRetryOnFailure();
                    providerOptions.MigrationsAssembly("Persistence");
                });
            }, ServiceLifetime.Scoped
            );
            services.AddDataProtection().PersistKeysToDbContext<CatalogDbContext>();
        }

        public static async Task MigrateCatalogDbToLatestVersionAsync(this IServiceScopeFactory scopeFactory)
        {
            await using var serviceScope = scopeFactory.CreateAsyncScope();
            await using var cataglogDbContext = serviceScope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            await cataglogDbContext.MigrateToLatestVersionAsync();
        }
        public static async Task RunCatalogDataSeederAsync(this IServiceScopeFactory scopeFactory)
        {
            await using var serviceScope = scopeFactory.CreateAsyncScope();
            var catalogDataSeeder = serviceScope.ServiceProvider.GetRequiredService<CatalogDataSeeder>();
            await catalogDataSeeder.SeedAsync();
        }
    }
}
