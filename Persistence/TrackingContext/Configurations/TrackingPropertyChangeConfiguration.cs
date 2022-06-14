using Domain.Entities.Tracking;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.TrackingContext.Configurations
{
    internal sealed class TrackingPropertyChangeConfiguration : IEntityTypeConfiguration<TrackingPropertyChange>
    {
        public void Configure(EntityTypeBuilder<TrackingPropertyChange> builder)
        {
            builder.ToTable("TrackingPropertyChanges");
            builder.HasKey(p => new { p.Id, p.ActionId });
            builder.Property(d => d.Id).IsRequired().ValueGeneratedOnAdd().HasColumnType("int");

            builder.HasOne(p => p.TrackingAction).WithMany(c => c.TrackingPropertyChanges)
                .HasForeignKey(p => new { p.ActionId })
                    .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
