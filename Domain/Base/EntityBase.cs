namespace Domain.Base
{
    public abstract class EntityBase<TKey> : IEntityBase<TKey>
    {
#nullable disable
        public virtual TKey Id { get; set; }
    }
}
