using Domain.Base;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities.Catalog
{
    public class ApplicationUser : IdentityUser, IAuditEntity
    {
        public int? TenantId { get; set; }
        public string CustomTag { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime? RefreshTokenExpires { get; set; }

        public virtual DateTime CreatedDate { get; set; }
        public virtual string CreatedBy { get; set; } = string.Empty;
        public virtual DateTime? UpdatedDate { get; set; }
        public virtual string UpdatedBy { get; set; } = string.Empty;

        public virtual AppTenant? Tenant { get; set; }
    }

    public class ApplicationRole : IdentityRole
    {
        public string CustomTag { get; set; } = string.Empty;

    }
}
