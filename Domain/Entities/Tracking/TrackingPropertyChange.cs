namespace Domain.Entities.Tracking
{
    public partial class TrackingPropertyChange
    {
        public virtual int Id { get; set; }
        public virtual int ActionId { get; set; }

        public virtual string Entity { get; set; }
        public virtual string Property { get; set; }
        public virtual int? OldEnumValue { get; set; }

        public virtual string OldString { get; set; }

        public virtual int? NewEnumValue { get; set; }

        public virtual string NewString { get; set; }

        public virtual string EntityInstanceId { get; set; }

        public virtual TrackingAction TrackingAction
        {
            get;
            set;
        }
    }
}
