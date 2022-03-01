using Microsoft.Extensions.DependencyInjection.Extensions;
using Persistence;
using Persistence.CatalogContext;
using Persistence.CatalogContext.CatalogSeed;
using Serilog;
using Services;
using Services.Abstractions;
using Services.Abstractions.Shared;
using Services.Shared;
using Web.Extensions;
using Web.Middleware;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddControllers().AddApplicationPart(typeof(Presentation.AssemblyReference).Assembly);

    builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    builder.Services.AddTransient<ExceptionHandlingMiddleware>();

    builder.Services.TryAdd(ServiceDescriptor.Singleton<ILoggerFactory, LoggerFactory>());
    builder.Services.TryAdd(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(Logger<>)));

    builder.Services.AddScoped(typeof(ITokenService), typeof(TokenService));
    builder.Services.AddScoped(typeof(IServiceManager), typeof(ServiceManager));
    builder.Services.AddAppIdentity(builder.Configuration);
    builder.Services.AddScoped<CatalogDataSeeder>();

    builder.Services.AddPersistenceContext(builder.Configuration);
    builder.Services.AddDataProtection();
    builder.Host.UseSerilog((ctx, lc) => lc.WriteTo.Console());

    var app = builder.Build();
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<TenantIdentifierMiddleware>();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    var scopeFactory = app.Services?.GetService<IServiceScopeFactory>();
    if (scopeFactory != null)
    {
        await scopeFactory.MigrateCatalogDbToLatestVersionAsync();
        await scopeFactory.RunCatalogDataSeederAsync();
    }

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
