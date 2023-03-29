using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Utilities.Extensions
{
    public static class ConfigureCors
    {
        public static bool UseCors = false;
        public static string AppCorsPolicy = string.Empty;
        public static void ConfigureAppCors(this IServiceCollection services, IConfiguration configuration)
        {
            IConfigurationSection confPolicy = configuration.GetSection("CorsOption:PolicyName");
            IConfigurationSection confOrigins = configuration.GetSection("CorsOption:WithOrigins");

            if (confOrigins.Exists() && !string.IsNullOrEmpty(confPolicy.Value) && confOrigins.Exists())
            {
                AppCorsPolicy = confPolicy.Value;
                string[]? origins = confOrigins.Get<string[]>();
                if (origins != null && origins.Length > 0)
                {
                    services.AddCors(options =>
                    {
                        options.AddPolicy(name: AppCorsPolicy,
                                          policy =>
                                          {
                                              if (!origins.Contains("*"))
                                                  policy.WithOrigins(origins).AllowCredentials();
                                              else
                                                  policy.AllowAnyOrigin();

                                              policy.AllowAnyMethod().AllowAnyHeader();
                                          });
                    });
                    UseCors = true;
                }
            }
        }
    }
}