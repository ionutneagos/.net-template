using Persistence;
using Persistence.CatalogContext;
using Persistence.CatalogContext.CatalogSeed;
using Persistence.TrackingContext;

namespace Web.Extensions
{
    public static class ConfigureAppPersistence
    {
        public static void AddAppPersistence(this WebApplicationBuilder builder)
        {
            string? catalogConnectionString = builder.Configuration["Database:CatalogConnectionString"];
            if (!string.IsNullOrEmpty(catalogConnectionString))
            {
                builder.Services.AddScoped<CatalogDataSeeder>();
                builder.Services.AddPersistenceContext(builder.Configuration);
            }

            string? trackingConnectionString = builder.Configuration["Database:TrackingConnectionString"];
            if (!string.IsNullOrEmpty(trackingConnectionString))
            {
                builder.Services.AddTrackingContext(builder.Configuration);
            }
        }

        public static async Task InitAppDataAsync(this WebApplication app)
        {
            string? catalogConnectionString = app.Configuration["Database:CatalogConnectionString"];
            IServiceScopeFactory? scopeFactory = app.Services?.GetService<IServiceScopeFactory>();
            if (scopeFactory == null)
                return;

            if (!string.IsNullOrEmpty(catalogConnectionString))
            {
                await scopeFactory.MigrateCatalogDbToLatestVersionAsync();
                await scopeFactory.RunCatalogDataSeederAsync();
            }

            string? trackingConnectionString = app.Configuration["Database:TrackingConnectionString"];
            if (!string.IsNullOrEmpty(trackingConnectionString))
            {
                await scopeFactory.MigrateTrackingDbToLatestVersionAsync();
            }
        }
    }
}
