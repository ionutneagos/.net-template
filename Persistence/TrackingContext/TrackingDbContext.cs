using Domain.Constants;
using Domain.Entities.Tracking;
using Microsoft.EntityFrameworkCore;
using Persistence.CatalogContext;
using Persistence.Common;
using Persistence.TrackingContext.Configurations;

#nullable disable
namespace Persistence.TrackingContext
{
    public class TrackingDbContext : DbContext, ITrackingDbContext
    {
        public string Provider => "System.Data.SqlClient";
        public string ContextName => ContextConfiguration.TrackingContextName;
        public TrackingDbContext(DbContextOptions<TrackingDbContext> options)
       : base(options)
        {
        }

        public DbSet<TrackingActivity> TrackingActivities { get; set; }
        public DbSet<TrackingPropertyChange> TrackingPropertyChanges { get; set; }
        public DbSet<TrackingAction> TrackingActions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {

            builder.ApplyConfiguration(new TrackingActionConfiguration());
            builder.ApplyConfiguration(new TrackingActivityConfiguration());
            builder.ApplyConfiguration(new TrackingPropertyChangeConfiguration());

            //filter types within the assembly based on context name
            builder.ApplyConfigurationsFromAssembly(typeof(TrackingDbContext).Assembly, x => x.Namespace.Contains(ContextName));
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
