namespace Domain.Entities.Tracking
{
    public partial class TrackingAction
    {
        public virtual int Id { get; set; }

        public virtual int ActivityId { get; set; }

        public virtual string Entity { get; set; }

        public virtual string EntityInstanceId { get; set; }

        public virtual string Command { get; set; }

        public virtual TrackingActivity TrackingActivity
        {
            get;
            set;
        }
        public virtual ICollection<TrackingPropertyChange> TrackingPropertyChanges
        {
            get;
            set;
        }
    }
}
