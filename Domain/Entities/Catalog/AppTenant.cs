using Domain.Base;

namespace Domain.Entities.Catalog
{
    public class AppTenant : AuditEntityBase<int>
    {
        public string Name { get; set; } = string.Empty;
        public string NormalizedName { get; set; } = string.Empty;

        public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}
