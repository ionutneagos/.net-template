namespace Contracts.Catalog.AppUser.Response
{
    public class UserResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? CustomTag { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime CreatedDate { get; set; }

        public int? TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
    }
}
