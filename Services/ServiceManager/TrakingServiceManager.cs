using Domain.Constants;
using Domain.Entities.Tracking;
using Domain.Repositories;
using Services.Tracking;

namespace Services
{
    public partial class ServiceManager
    {
        private static string TrackingContextName => ContextConfiguration.TrackingContextName;
        private bool IsEntityTrackingEnabled;

        private void InitEntityTracking()
        {
            try
            {
                string trackingContextName = ContextConfiguration.TrackingContextName;

                if (!_contextPool.ContainsKey(trackingContextName))
                    RepositoryResolver<TrackingActivity>(ContextConfiguration.TrackingContextName);

                IRepositoryContext trackingContext = _contextPool[trackingContextName];
                IsEntityTrackingEnabled = trackingContext != null;
            }
            catch (Exception)
            {
                IsEntityTrackingEnabled = false;
            }
        }

        public IEnumerable<TrackingAction>? GetTrackedActionsFromEntity(object entity)
        {
            if (IsEntityTrackingEnabled)
                return new EntityTracker(_contextPool[TrackingContextName]).GetTrackedActionsFromEntity(entity);
            else
                return null;
        }

        public async Task<List<TrackingAction>?> GetTrackedActionsFromEntityAsync(object entity, CancellationToken cancellationToken = default)
        {
            if (IsEntityTrackingEnabled)
                return await new EntityTracker(_contextPool[TrackingContextName]).GetTrackedActionsFromEntityAsync(entity, cancellationToken);
            else
                return null;
        }

        #region Private Methods
        private void CommitTrackingTransaction(EntityTrackingTransaction transaction)
        {
            if (transaction != null && IsEntityTrackingEnabled)
            {
                transaction.Tracker.PersistActions(transaction);
            }
        }

        private async Task CommitTrackingTransactionAsync(EntityTrackingTransaction transaction, CancellationToken cancellationToken = default)
        {
            if (transaction != null && IsEntityTrackingEnabled)
            {
                await transaction.Tracker.PersistActionsAsync(transaction, cancellationToken);
            }
        }

        private EntityTrackingTransaction? TrackContext(IRepositoryContext contextToTrack)
        {
            EntityTrackingTransaction? transaction = null;
            if (IsEntityTrackingEnabled)
            {
                string? tenantIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ContextConfiguration.TenantIdClaim)?.Value;
                int? tenantId = null;
                if (!string.IsNullOrEmpty(tenantIdClaim))
                    tenantId = int.Parse(tenantIdClaim);

                IRepositoryContext trackingContext = _contextPool[TrackingContextName];
                EntityTracker entityTracker = new(trackingContext);
                transaction = entityTracker.TrackEntities(contextToTrack, User?.Identity?.Name, tenantId);
            }
            return transaction;
        }
        #endregion
    }
}
