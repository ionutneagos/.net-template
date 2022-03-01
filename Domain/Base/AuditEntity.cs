namespace Domain.Base
{
    public abstract class AuditEntity : IAuditEntity
    {
        public virtual DateTime CreatedDate { get; set; }
        public virtual string CreatedBy { get; set; } = string.Empty;
        public virtual DateTime? UpdatedDate { get; set; }
        public virtual string UpdatedBy { get; set; } = string.Empty;
    }
}
