namespace Contracts.Catalog.AppTenant.Response
{
    public class TenantResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}
