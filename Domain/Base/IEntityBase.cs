namespace Domain.Base
{
    public interface IEntityBase<TKey>
    {
        TKey Id { get; set; }
    }
}
