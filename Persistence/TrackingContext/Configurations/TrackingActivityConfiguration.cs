using Domain.Entities.Tracking;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.TrackingContext.Configurations
{
    internal sealed class TrackingActivityConfiguration : IEntityTypeConfiguration<TrackingActivity>
    {
        public void Configure(EntityTypeBuilder<TrackingActivity> builder)
        {
            builder.ToTable("TrackingActivities");
            builder.HasKey(d => d.Id);
            builder.Property(d => d.Id).IsRequired().ValueGeneratedOnAdd().HasColumnType("int");
        }
    }
}
