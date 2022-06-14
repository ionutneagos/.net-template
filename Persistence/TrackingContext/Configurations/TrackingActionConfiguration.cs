using Domain.Entities.Tracking;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.TrackingContext.Configurations
{
    internal sealed class TrackingActionConfiguration : IEntityTypeConfiguration<TrackingAction>
    {
        public void Configure(EntityTypeBuilder<TrackingAction> builder)
        {
            builder.ToTable("TrackingActions");
            builder.HasKey(d => d.Id);
            builder.Property(d => d.Id).IsRequired().ValueGeneratedOnAdd().HasColumnType("int");

            builder.HasOne(x => x.TrackingActivity).WithMany(c => c.TrackingActions)
                .HasForeignKey(p => new { p.ActivityId })
                    .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
