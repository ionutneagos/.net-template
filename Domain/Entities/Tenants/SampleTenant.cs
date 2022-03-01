using Domain.Base;

namespace Domain.Entities.Tenant
{
    public class SampleTenant : AuditEntityBase<Guid>
    {
        public string Name { get; set; } = string.Empty;
    }
}
