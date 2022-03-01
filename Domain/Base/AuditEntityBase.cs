#nullable disable
namespace Domain.Base
{
    public abstract class AuditEntityBase<TKey> : AuditEntity, IEntityBase<TKey>
    {
        public virtual TKey Id { get; set; }
    }
}
