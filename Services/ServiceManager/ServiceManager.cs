using Domain.Repositories;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.Abstractions.Shared;
using Services.Shared;
using System.Security.Principal;

#nullable disable
namespace Services
{
    public partial class ServiceManager : IServiceManager
    {
        public IConfiguration Configuration { get; }
        public IPrincipal User => _httpContextAccessor.HttpContext?.User;
        public IDataEncryptionService DataEncryptionService => _lazyDataEncryptionService.Value;


        internal readonly Dictionary<string, IRepositoryContext> _contextPool = new();
        internal readonly IHttpContextAccessor _httpContextAccessor;

        private readonly IServiceProvider _serviceProvider;
        private Lazy<IDataEncryptionService> _lazyDataEncryptionService;
        private readonly Lazy<ILoggerFactory> _lazyLoggerFactory;
        private ILogger ServiceLogger;

        public ServiceManager(IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor,
             ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _serviceProvider = serviceProvider;
            _lazyLoggerFactory = new Lazy<ILoggerFactory>(() => loggerFactory);
            Configuration = configuration;

            InitSharedServices();
            InitCatalogServices();
            InitTenantServices();
        }

        private void InitSharedServices()
        {
            _lazyDataEncryptionService = new Lazy<IDataEncryptionService>(() => new DataEncryptionService(_serviceProvider.GetDataProtectionProvider()));
        }

        public void Commit(string contextName = null)
        {
            if (string.IsNullOrEmpty(contextName))
                _contextPool.Values.ToList().ForEach(context => context.Commit(User?.Identity?.Name));
            else
                _contextPool[contextName].Commit(User?.Identity?.Name);
        }

        public async Task CommitAsync(string contextName = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(contextName))
                _contextPool.Values.ToList().ForEach(async context => await context.CommitAsync(User?.Identity?.Name, cancellationToken));
            else
                await _contextPool[contextName].CommitAsync(User?.Identity?.Name, cancellationToken);
        }

        #region Logger 
        public void Log<T>(LogLevel level, string message, params object[] args)
        {
            if (string.IsNullOrEmpty(message) && args is null)
                return;

            if (ServiceLogger == null || (ServiceLogger.GetType().GenericTypeArguments.Length > 0 &&
                ServiceLogger.GetType().GenericTypeArguments[0]?.Name != typeof(T).Name))

                ServiceLogger = SetLoger<T>();

            ServiceLogger.Log(level, message, args);
        }
        public ILogger<T> SetLoger<T>()
        {
            ServiceLogger = _lazyLoggerFactory.Value.CreateLogger<T>();
            return (ILogger<T>)ServiceLogger;
        }
        public void Log(LogLevel level, string message, params object[] args)
        {
            if (string.IsNullOrEmpty(message) && args is null)
                return;
            ServiceLogger ??= SetLoger<ServiceManager>();

            ServiceLogger.Log(level, message, args);
        }
        #endregion

        #region Protected Methods
        protected IGenericRepository<T> RepositoryResolver<T>(string contextName) where T : class
        {
            var repository = _serviceProvider.GetServices<IGenericRepository<T>>()
                .FirstOrDefault(x => x.Context.ContextName == contextName);

            if (repository != null)
            {
                if (!_contextPool.ContainsKey(contextName))
                    _contextPool.Add(contextName, repository.Context);

                return repository;
            }
            return null;
        }
        #endregion

        #region Dispose Methods
        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    foreach (var context in _contextPool.Values)
                    {
                        if (context != null)
                            context.Dispose();
                    }
                }
            }
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
