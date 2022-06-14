using Domain.Constants;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Persistence;
using Persistence.CatalogContext;
using Persistence.CatalogContext.CatalogSeed;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using Services;
using Services.Abstractions;
using Services.Abstractions.Shared;
using Services.Shared;
using Web.Extensions;
using Web.Middleware;
using Web.Utils;

try
{
    var builder = WebApplication.CreateBuilder(args);
    //register azure key vault as top dependency
    builder.AddAzureKeyVault();
    builder.Services.ConfigureAppCors(builder.Configuration);

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.AddBearerSecurityDefinition();
        c.OperationFilter<SwaggerDefaultValues>();
    });

    builder.Services.AddControllers(options =>
    {
        options.InputFormatters.Insert(0, JsonPatchFormatter.GetJsonPatchInputFormatter());

    }).AddOData(options => options.Select().Filter().OrderBy().Expand().Count().SetMaxTop(ApiConfiguration.ODataOptionsMaxTopValue)
                            .EnableNoDollarQueryOptions = true)
     .AddApplicationPart(typeof(Presentation.AssemblyReference).Assembly);

    builder.Services.RemoveODataFormatters();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

    builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    builder.Services.AddTransient<ExceptionHandlingMiddleware>();

    builder.Services.TryAdd(ServiceDescriptor.Singleton<ILoggerFactory, LoggerFactory>());
    builder.Services.TryAdd(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(Logger<>)));

    builder.Services.AddScoped(typeof(ITokenService), typeof(TokenService));
    builder.Services.AddScoped(typeof(IServiceManager), typeof(ServiceManager));
    builder.Services.AddHttpClient(Microsoft.Extensions.Options.Options.DefaultName, client =>
    {
        client.Timeout = TimeSpan.FromMinutes(15);
    }).AddPolicyHandler(GetRetryPolicy());

    builder.Services.AddAppIdentity(builder.Configuration);
    builder.AddAppPersistence();

    builder.Host.UseSerilog((ctx, lc) => lc.WriteTo.Console());
    var app = builder.Build();

    app.MapSwagger();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        if (!app.Environment.IsDevelopment())
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            options.RoutePrefix = string.Empty;
        }
    });

    app.UseHttpsRedirection();
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<TenantIdentifierMiddleware>();

    if (ConfigureCors.UseCors)
    {
        app.UseCors(ConfigureCors.AppCorsPolicy);
    }

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

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