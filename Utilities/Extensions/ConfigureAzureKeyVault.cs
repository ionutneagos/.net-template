using Azure.Core;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Utilities.Extensions
{
    public static class ConfigureAzureKeyVault
    {
        public static void AddAzureKeyVault(this WebApplicationBuilder builder)
        {
            string? keyVaultName = builder.Configuration["KeyVaultName"];

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
            ConfigurationManager configuration = builder.Configuration;

            if (builder.Environment.IsDevelopment())
                credentials = new ClientSecretCredential(configuration["AzureAd:TenantId"],
                    configuration["AzureAd:ClientId"], configuration["AzureAd:ClientSecret"]);
            else
                credentials = new ManagedIdentityCredential();

            return credentials;
        }
    }
}
