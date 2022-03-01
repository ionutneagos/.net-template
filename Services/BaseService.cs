using Domain.Repositories;
using Services.Abstractions;

namespace Services
{
    internal class BaseService<T> where T : class
    {
        protected IGenericRepository<T> repository;
        internal readonly IServiceManager serviceManager;

        public BaseService(IServiceManager serviceManager, IGenericRepository<T> repository)
        {
            this.repository = repository;
            this.serviceManager = serviceManager;
        }
    }
}
