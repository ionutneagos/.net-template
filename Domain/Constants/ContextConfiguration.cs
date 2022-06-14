namespace Domain.Constants
{
    public static class ContextConfiguration
    {
        public const string CatalogContextName = "CatalogContext";
        public const string TenantContextName = "TenantContext";
        public const string TrackingContextName = "TrackingContext";

        public const string TenantIdClaim = "TenantIdClaim";
        public const string TenantConnectionString = "TenantConnectionString";
        public const string TenantDbNamePlaceHolder = "{$TenantId}";
    }
}
