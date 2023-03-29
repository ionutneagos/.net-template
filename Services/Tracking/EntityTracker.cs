using Domain.Base;
using Domain.Entities.Tracking;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.ComponentModel.DataAnnotations;
using System.Text;

#nullable disable
namespace Services.Tracking
{
    public class EntityTracker
    {
        #region Variables
        private readonly IRepositoryContext context;
        #endregion

        #region Constructor
        public EntityTracker(IRepositoryContext context)
        {
            this.context = context;
        }
        #endregion

        #region Methods
        internal IEnumerable<TrackingAction> GetTrackedActionsFromEntity(object entity)
        {
            string entityKey = GetEntityInstanceId(entity);
            string entityName = GetEntityType(entity).FullName;
            IEnumerable<TrackingAction> actions = null;
            DbContext context = this.context as DbContext;
            if (context != null)
            {
                actions = context.Set<TrackingAction>()
                    .Where(a => a.EntityInstanceId == entityKey)
                    .Where(a => a.Entity == entityName)
                    .Include(a => a.TrackingActivity)
                    .Include(a => a.TrackingPropertyChanges)
                    .OrderByDescending(a => a.TrackingActivity.ActivityDate)
                    .AsNoTracking();
            }
            return actions;
        }

        internal async Task<List<TrackingAction>> GetTrackedActionsFromEntityAsync(object entity, CancellationToken cancellationToken = default)
        {
            string entityKey = GetEntityInstanceId(entity);
            string entityName = GetEntityType(entity).FullName;
            List<TrackingAction> actions = new();
            DbContext context = this.context as DbContext;
            if (context != null)
            {
                actions = await context.Set<TrackingAction>()
                    .Where(a => a.EntityInstanceId == entityKey)
                    .Where(a => a.Entity == entityName)
                    .Include(a => a.TrackingActivity)
                    .Include(a => a.TrackingPropertyChanges)
                    .OrderByDescending(a => a.TrackingActivity.ActivityDate)
                    .ToListAsync(cancellationToken);
            }
            return actions;
        }

        internal EntityTrackingTransaction TrackEntities(IRepositoryContext contextToTracking, string userId, int? tenantId)
        {
            EntityTrackingTransaction transaction = null;
            if (contextToTracking != null || !contextToTracking.Equals(context))
            {
                try
                {
                    transaction = new EntityTrackingTransaction
                    {
                        Tracker = this,
                        TrackingContext = context,
                        ContextToTrack = contextToTracking
                    };
                    ProcessContextActivity(userId, tenantId, transaction);
                }
                catch (Exception)
                {
                    //Log exception
                }
            }
            return transaction;
        }
        internal void PersistActions(EntityTrackingTransaction transaction)
        {
            foreach (EntityEntry item in transaction.AddedEntities.Keys)
            {
                transaction.AddedEntities[item].EntityInstanceId = GetEntityInstanceId(item.Entity);
            }
            transaction.TrackingContext.Commit(null);
        }
        internal Task PersistActionsAsync(EntityTrackingTransaction transaction, CancellationToken cancellationToken = default)
        {
            foreach (EntityEntry item in transaction.AddedEntities.Keys)
            {
                transaction.AddedEntities[item].EntityInstanceId = GetEntityInstanceId(item.Entity);
            }
            return transaction.TrackingContext.CommitAsync(null, cancellationToken);
        }
        #endregion

        #region  Protected Methods
        protected void ProcessContextActivity(string userId, int? tenantId, EntityTrackingTransaction transaction)
        {
            TrackingActivity activity = new TrackingActivity();
            activity.ActivityDate = DateTime.Now;
            activity.UserId = userId ?? string.Empty;
            activity.TenantId = tenantId;
            activity.TrackingActions = new List<TrackingAction>();
            ProcessActions(transaction, activity);
            if (transaction.TrackingContext != null && activity.TrackingActions.Count > 0)
            {
                DbContext trackingContext = transaction.TrackingContext as DbContext;
                trackingContext.Set<TrackingActivity>().Add(activity);
            }
        }
        protected void ProcessActions(EntityTrackingTransaction transaction, TrackingActivity activity)
        {
            foreach (EntityEntry item in transaction.GetEntries())
            {
                TrackingAction action = new()
                {
                    TrackingPropertyChanges = new List<TrackingPropertyChange>(),
                    Command = Enum.GetName(typeof(EntityState), item.State)
                };
                if (item.State == EntityState.Unchanged)
                {
                    continue;
                }
                action.Entity = GetEntityType(item.Entity).FullName;
                action.EntityInstanceId = GetEntityInstanceId(item.Entity);
                action.TrackingActivity = activity;
                activity.TrackingActions.Add(action);
                if (item.State == EntityState.Deleted)
                {
                    continue;
                }
                if (item.State == EntityState.Added)
                {
                    transaction.AddedEntities.Add(item, action);
                }
                ProcessPropertyChanges(item, action);
            }
        }

        private void ProcessPropertyChanges(EntityEntry item, TrackingAction action)
        {
            IEnumerable<string> excludedProperties = typeof(AuditEntity).GetProperties().Select(t => t.Name);
            List<string> changedProperites = item.Properties
                    .Select(x => x.Metadata.Name).Where(v => !excludedProperties.Contains(v)).ToList();

            foreach (string property in changedProperites)
            {
                PropertyEntry propertyEntry = item.Property(property);
                string currentValue, originalValue = string.Empty;
                currentValue = propertyEntry.CurrentValue?.ToString();
                if (item.State != EntityState.Added)
                {
                    originalValue = item.GetDatabaseValues().GetValue<object>(property)?.ToString();
                }

                if (currentValue != originalValue)
                {
                    TrackingPropertyChange change = new();
                    Type propertyType = GetEntityType(item.Entity);
                    change.Entity = GetEntityType(item.Entity).FullName;
                    change.EntityInstanceId = GetEntityInstanceId(item.Entity);
                    change.Property = property;
                    change.TrackingAction = action;
                    change.NewString = change.OldString = string.Empty;
                    if (!string.IsNullOrEmpty(currentValue))
                    {
                        change.NewString = propertyEntry.CurrentValue.ToString();
                        if (propertyType.IsEnum)
                            change.NewString = Enum.GetName(propertyType, currentValue);
                    }
                    if (!string.IsNullOrEmpty(originalValue))
                    {
                        change.OldString = originalValue;
                        if (propertyType.IsEnum)
                            change.OldEnumValue = int.Parse(originalValue);
                    }
                    action.TrackingPropertyChanges.Add(change);
                }
            }
        }
        protected Type GetEntityType(object entity)
        {
            Type entityType = entity.GetType();
            if (entityType.FullName.StartsWith("System.Data.Entity.DynamicProxies"))
            {
                entityType = entityType.BaseType;
            }
            return entityType;
        }
        protected string GetEntityInstanceId(object entity)
        {
            StringBuilder key = new StringBuilder();
            Type entityType = GetEntityType(entity);
            if (entityType.GetProperties().Where(p => p.GetCustomAttributes(typeof(KeyAttribute), false).Count() > 0).Count() > 0)
            {
                foreach (System.Reflection.PropertyInfo item in entityType.GetProperties().Where(p => p.GetCustomAttributes(typeof(KeyAttribute), false).Count() > 0))
                {
                    key.Append(string.Format("{0}:{1};", item.Name, item.GetValue(entity, null)));
                }
            }
            else
            {
                System.Reflection.PropertyInfo keyId = entityType.GetProperties().Where(p => p.Name.ToLower() == "id").FirstOrDefault();
                if (keyId != null)
                {
                    key.Append(string.Format("{0}:{1};", keyId.Name, keyId.GetValue(entity, null)));
                }
            }
            return key.ToString();
        }
        #endregion
    }
}
