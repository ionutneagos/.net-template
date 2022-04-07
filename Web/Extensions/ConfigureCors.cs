namespace Web.Extensions
{
    internal static class ConfigureCors
    {
        internal static bool UseCors = false;
        internal static string AppCorsPolicy = string.Empty;
        public static void ConfigureAppCors(this IServiceCollection services, IConfiguration configuration)
        {
            var confPolicy = configuration.GetSection("CorsOption:PolicyName");
            var confOrigins = configuration.GetSection("CorsOption:WithOrigins");

            if (confOrigins.Exists() && confOrigins.Exists())
            {
                AppCorsPolicy = confPolicy.Value;
                var origins = confOrigins.Get<string[]>();
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
