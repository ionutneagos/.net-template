using Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.CatalogContext.Configurations
{
    internal sealed class TenantConfiguration : IEntityTypeConfiguration<AppTenant>
    {
        public void Configure(EntityTypeBuilder<AppTenant> builder)
        {
            builder.ToTable(nameof(AppTenant));

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).ValueGeneratedOnAdd();

            builder.HasMany(e => e.Users)
              .WithOne(e => e.Tenant)
              .HasForeignKey(ur => ur.TenantId)
              .IsRequired(false);
        }
    }
}
