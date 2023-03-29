using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Services.Abstractions.Catalog;
using Services.Abstractions.Shared;
using Services.Abstractions.Tenant;
using Services.Shared;
using System.Security.Principal;

namespace Services.Abstractions
{
    public interface IServiceManager : IDisposable
    {
        string EnvironmentName { get; }
        IConfiguration Configuration { get; }
        IMemoryCache MemoryCache { get; }
        IPrincipal User { get; }

        #region Shared Services
        IDataEncryptionService DataEncryptionService { get; }
        IMappingService MappingService { get; }
        #endregion

        #region Catalog Services
        ISampleService SampleService { get; }
        IAppUserService AppUserService { get; }
        IAppTenantService AppTenantService { get; }
        #endregion

        #region Tenant Services
        ISampleTenantService SampleTenantService { get; }
        #endregion

        #region Logger 
        /// <summary>
        /// Set Service Manager Logger 
        /// </summary>
        ILogger<T> SetLoger<T>();
        /// <summary>
        /// Set Service Manager Logger and add logs
        /// </summary>
        void Log<T>(LogLevel level = LogLevel.Debug, string? message = null, params object[]? args);
        /// <summary>
        /// Log using  Service Manager Logger. If Service Manager Logger is not setted the
        /// new instance of Logger<oftype(ServiceManager)> would be createds
        /// </summary>
        void Log(LogLevel level = LogLevel.Debug, string? message = null, params object[]? args);
        #endregion

        void Commit(string? contextName = null);

        Task CommitAsync(string? contextName = null, CancellationToken cancellationToken = default);
    }
}