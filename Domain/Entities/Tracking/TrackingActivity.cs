namespace Domain.Entities.Tracking
{
    public partial class TrackingActivity
    {
        public virtual int Id { get; set; }

        public virtual string UserId { get; set; }

        public virtual DateTime ActivityDate { get; set; }

        public virtual int? TenantId { get; set; }

        public virtual ICollection<TrackingAction> TrackingActions
        {
            get;
            set;
        }
    }
}
