using Domain.Constants;
using EFCore.AutomaticMigrations;
using Microsoft.EntityFrameworkCore;
using Persistence.TenantContext;

namespace Web.Middlewares
{
    public class TenantIdentifierMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantIdentifierMiddleware(RequestDelegate next, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _next = next;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            // Get tenant id from token
            string? tenantId = httpContext.User.FindFirst(ContextConfiguration.TenantIdClaim)?.Value;
            if (!string.IsNullOrWhiteSpace(tenantId))
            {
                string tenantConnectionString = GetTenantConnectionString(tenantId);
                if (!string.IsNullOrWhiteSpace(tenantConnectionString))
                {
                    httpContext.Items[ContextConfiguration.TenantConnectionString] = tenantConnectionString;
                    using TenantDbContext tenantDbContext = new(new DbContextOptions<TenantDbContext>(), _httpContextAccessor);
                    await tenantDbContext.MigrateToLatestVersionAsync();
                }
            }
            await _next.Invoke(httpContext);
        }

        private string GetTenantConnectionString(string tenantId)
        {
            string? tenantConnectionStringTemplate = _configuration["Database:TenantConnectionString"];
            if (tenantConnectionStringTemplate == null)
                return string.Empty;
            return tenantConnectionStringTemplate.Replace(ContextConfiguration.TenantDbNamePlaceHolder, tenantId);
        }
    }
}