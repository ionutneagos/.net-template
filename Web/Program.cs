using Domain.Constants;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using Services;
using Services.Abstractions;
using Services.Abstractions.Shared;
using Services.Shared;
using Utilities.Extensions;
using Utilities.Formatters;
using Utilities.Swagger;
using Web.Extensions;
using Web.Middlewares;
try
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    // For Serilog
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext()
        .CreateLogger();
    builder.Host.UseSerilog(Log.Logger);

    //register azure key vault as top dependency
    builder.AddAzureKeyVault();

    builder.Services.ConfigureAppCors(builder.Configuration);

    builder.Services.AddControllers(options =>
    {
        options.InputFormatters.Insert(0, JsonPatchFormatter.GetJsonPatchInputFormatter());

    }).AddOData
        (options =>
            options.Select()
                .Filter()
                .OrderBy()
                .Expand()
                .Count()
                .SetMaxTop(ApiConfiguration.ODataOptionsMaxTopValue)
                .EnableNoDollarQueryOptions = true)
     .AddApplicationPart(typeof(Presentation.AssemblyReference).Assembly);

    builder.Services.RemoveODataFormatters();

    builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

    builder.Services.AddTransient<ExceptionHandlingMiddleware>();

    builder.Services.TryAdd(ServiceDescriptor.Singleton<ILoggerFactory, LoggerFactory>());
    builder.Services.TryAdd(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(Logger<>)));

    builder.Services.AddScoped(typeof(ITokenService), typeof(TokenService));
    builder.Services.AddScoped(typeof(IServiceManager), typeof(ServiceManager));

    builder.Services.AddMemoryCache();

    builder.Services.AddHttpClient(Microsoft.Extensions.Options.Options.DefaultName, client =>
    {
        client.Timeout = TimeSpan.FromMinutes(15);
    }).AddPolicyHandler(GetRetryPolicy());

    builder.Services.ConfigureRateLimit(builder.Configuration);

    builder.Services.AddAppIdentity(builder.Configuration);

    builder.AddAppPersistence();

    builder.Services.ConfigureApiVersioning(builder.Configuration);


    WebApplication app = builder.Build();

    app.UseMiddleware<ExceptionHandlingMiddleware>();

    IApiVersionDescriptionProvider provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                    description.GroupName.ToUpperInvariant());
            }
        });
    }

    app.UseHttpsRedirection();


    if (ConfigureCors.UseCors)
    {
        app.UseCors(ConfigureCors.AppCorsPolicy);
    }

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseMiddleware<TenantIdentifierMiddleware>();

    app.MapControllers();
    app.UseRateLimiter();
    await app.InitAppDataAsync();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions.HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}