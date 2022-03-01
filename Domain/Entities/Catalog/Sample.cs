using Domain.Base;

namespace Domain.Entities.Catalog
{
    public class Sample : AuditEntityBase<Guid>
    {
        public string Name { get; set; } = string.Empty;
    }
}
