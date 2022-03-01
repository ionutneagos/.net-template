using Domain.Entities.Tenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.TenantContext.Configurations
{
    internal sealed class SampleTenantConfiguration : IEntityTypeConfiguration<SampleTenant>
    {
        public void Configure(EntityTypeBuilder<SampleTenant> builder)
        {
            builder.ToTable(nameof(SampleTenant));

            builder.HasKey(sample => sample.Id);
            builder.Property(sample => sample.Id).ValueGeneratedOnAdd();
        }
    }
}
