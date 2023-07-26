using Domain.Constants;
using Domain.Entities.Catalog;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Persistence.Common;

namespace Persistence.CatalogContext
{
    public class CatalogDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>,
        ICatalogDbContext, IDataProtectionKeyContext
    {
        public string Provider => "System.Data.SqlClient";
        public string ContextName => ContextConfiguration.CatalogContextName;
#nullable disable
        public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options)
        {
        }

        public DbSet<Sample> Samples { get; set; }
        public DbSet<AppTenant> Tenants { get; set; }
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("Users");
            });

            builder.Entity<ApplicationRole>(entity =>
            {
                entity.ToTable("Roles");
            });

            builder.Entity<IdentityUserRole<string>>(entity => { entity.ToTable("UserRoles"); });
            builder.Entity<IdentityUserClaim<string>>(entity => { entity.ToTable("UserClaims"); });
            builder.Entity<IdentityUserLogin<string>>(entity => { entity.ToTable("UserLogins"); });
            builder.Entity<IdentityRoleClaim<string>>(entity => { entity.ToTable("RoleClaims"); });
            builder.Entity<IdentityUserToken<string>>(entity => { entity.ToTable("UserTokens"); });

            builder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly, x => x.Namespace.Contains(ContextName));
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
