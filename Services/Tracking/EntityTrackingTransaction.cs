using Domain.Entities.Tracking;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

#nullable disable
namespace Services.Tracking
{
    public class EntityTrackingTransaction
    {
        public IRepositoryContext TrackingContext { get; set; }
        public IRepositoryContext ContextToTrack { get; set; }
        public EntityTracker Tracker { get; set; }
        public Dictionary<EntityEntry, TrackingAction> AddedEntities { get; set; } = new Dictionary<EntityEntry, TrackingAction>();

        public EntityTrackingTransaction()
        {
        }

        public IEnumerable<EntityEntry> GetEntries()
        {
            DbContext context = ContextToTrack as DbContext;
            IEnumerable<EntityEntry> entries = new List<EntityEntry>();
            if (context != null)
            {
                entries = context.ChangeTracker.Entries();
            }
            return entries;
        }
    }
}
