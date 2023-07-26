using Domain.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Persistence.Common;
using Persistence.TenantContext.Configurations;
using Persistence.TrackingContext;

#nullable disable
namespace Persistence.TenantContext
{
    public class TenantDbContext : DbContext, ITenantDbContext
    {
        public string Provider => "System.Data.SqlClient";
        public string ContextName => ContextConfiguration.TenantContextName;
        private readonly string tenantConnectionString;
        public TenantDbContext(DbContextOptions<TenantDbContext> options, IHttpContextAccessor httpContextAccessor)
        : base(options)
        {
            if (httpContextAccessor.HttpContext != null)
            {
                tenantConnectionString = httpContextAccessor.HttpContext.Items[ContextConfiguration.TenantConnectionString] as string;
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!string.IsNullOrEmpty(tenantConnectionString))
            {
                optionsBuilder.UseSqlServer(tenantConnectionString, providerOptions =>
                {
                    providerOptions.EnableRetryOnFailure();
                    providerOptions.MigrationsAssembly(typeof(TenantDbContext).Assembly.GetName().Name);

                });
                if (!optionsBuilder.IsConfigured)
                {
                    base.OnConfiguring(optionsBuilder);
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //filter types within the assembly based on context name
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TenantDbContext).Assembly, x => x.Namespace.Contains(ContextName));
        }

        public void Commit(dynamic userId)
        {
            ContextHelper.SaveChanges(this, userId);
        }

        public async Task CommitAsync(dynamic userId, CancellationToken cancellationToken = default)
        {
            await ContextHelper.SaveChangesAsync(this, userId, cancellationToken);
        }
        public void Rollback()
        {
            ContextHelper.Rollback(this);
        }
    }
}
