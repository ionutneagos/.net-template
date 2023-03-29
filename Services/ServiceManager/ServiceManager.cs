using Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Services.Abstractions;
using System.Security.Principal;

#nullable disable
namespace Services
{
    public partial class ServiceManager : IServiceManager
    {
        #region Protected Properties
        internal readonly Dictionary<string, IRepositoryContext> _contextPool = new();
        internal readonly IHttpContextAccessor _httpContextAccessor;
        internal readonly IServiceProvider _serviceProvider;
        internal readonly Lazy<ILoggerFactory> _lazyLoggerFactory;
        private ILogger ServiceLogger;
        #endregion

        #region Properties
        public IConfiguration Configuration { get; }
        public IPrincipal User => _httpContextAccessor.HttpContext?.User;
        public string EnvironmentName => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        public IMemoryCache MemoryCache { get; }
        #endregion

        public ServiceManager(IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor, ILoggerFactory loggerFactory, IMemoryCache memoryCache, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _serviceProvider = serviceProvider;
            Configuration = configuration;
            MemoryCache = memoryCache;

            _lazyLoggerFactory = new Lazy<ILoggerFactory>(() => loggerFactory);

            InitSharedServices();
            InitCatalogServices();
            InitTenantServices();
            InitEntityTracking();
        }

        #region Commit
        public void Commit(string contextName = null)
        {
            if (string.IsNullOrEmpty(contextName))
                _contextPool.Values.ToList().ForEach(context =>
                {
                    Tracking.EntityTrackingTransaction trackingTransaction = TrackContext(context);
                    context.Commit(User?.Identity?.Name);
                    CommitTrackingTransaction(trackingTransaction);
                });
            else
            {
                IRepositoryContext context = _contextPool[contextName];
                Tracking.EntityTrackingTransaction trackingTransaction = TrackContext(context);
                context.Commit(User?.Identity?.Name);
                CommitTrackingTransaction(trackingTransaction);
            }
        }
        public async Task CommitAsync(string contextName = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(contextName))
                _contextPool.Values.ToList().ForEach(async context =>
                {
                    Tracking.EntityTrackingTransaction trackingTransaction = TrackContext(context);
                    await context.CommitAsync(User?.Identity?.Name, cancellationToken);
                    await CommitTrackingTransactionAsync(trackingTransaction, cancellationToken);
                });
            else
            {
                IRepositoryContext context = _contextPool[contextName];
                Tracking.EntityTrackingTransaction trackingTransaction = TrackContext(context);
                await context.CommitAsync(User?.Identity?.Name, cancellationToken);
                await CommitTrackingTransactionAsync(trackingTransaction, cancellationToken);
            }
        }
        #endregion

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
            IGenericRepository<T> repository = _serviceProvider.GetServices<IGenericRepository<T>>()
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
                    foreach (IRepositoryContext context in _contextPool.Values)
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
