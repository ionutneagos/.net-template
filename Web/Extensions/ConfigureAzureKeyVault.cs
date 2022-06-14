using Azure.Core;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;

namespace Web.Extensions
{
    internal static class ConfigureAzureKeyVault
    {
        public static void AddAzureKeyVault(this WebApplicationBuilder builder)
        {
            var keyVaultName = builder.Configuration["KeyVaultName"];
            if (!string.IsNullOrEmpty(keyVaultName))
            {
                builder.Configuration.AddAzureKeyVault(
                             new Uri($"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/"),
                             GetTokenCredential(builder),
                             new AzureKeyVaultConfigurationOptions
                             {
                                 ReloadInterval = TimeSpan.FromMinutes(10),
                             });
            }
        }

        static TokenCredential GetTokenCredential(WebApplicationBuilder builder)
        {
            TokenCredential credentials;
            var configuration = builder.Configuration;

            if (builder.Environment.IsDevelopment())
                credentials = new ClientSecretCredential(configuration["AzureAd:TenantId"],
                    configuration["AzureAd:ClientId"], configuration["AzureAd:ClientSecret"]);
            else
                credentials = new ManagedIdentityCredential();

            return credentials;
        }
    }
}
