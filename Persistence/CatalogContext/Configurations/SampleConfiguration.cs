using Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Persistence.CatalogContext.Configurations
{
    internal sealed class SampleConfiguration : IEntityTypeConfiguration<Sample>
    {
        public void Configure(EntityTypeBuilder<Sample> builder)
        {
            builder.ToTable(nameof(Sample));

            builder.HasKey(sample => sample.Id);
            builder.Property(sample => sample.Id).ValueGeneratedOnAdd();
        }
    }
}
