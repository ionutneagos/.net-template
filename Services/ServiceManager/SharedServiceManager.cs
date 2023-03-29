using Microsoft.AspNetCore.DataProtection;
using Services.Abstractions.Shared;
using Services.Shared;

namespace Services
{
    public partial class ServiceManager
    {
        private Lazy<IDataEncryptionService> _lazyDataEncryptionService;
        private Lazy<IMappingService> _mappingService;

        private void InitSharedServices()
        {
            _lazyDataEncryptionService = new Lazy<IDataEncryptionService>(() => new DataEncryptionService(_serviceProvider.GetDataProtectionProvider()));
            _mappingService = new Lazy<IMappingService>(() => new MappingService());
        }

        public IDataEncryptionService DataEncryptionService => _lazyDataEncryptionService.Value;
        public IMappingService MappingService => _mappingService.Value;

    }
}
