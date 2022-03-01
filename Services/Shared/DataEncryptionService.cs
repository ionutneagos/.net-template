using Microsoft.AspNetCore.DataProtection;
using Services.Abstractions.Shared;

namespace Services.Shared
{
    public class DataEncryptionService : IDataEncryptionService
    {
        private readonly IDataProtectionProvider dataProtectionProvider;

        public DataEncryptionService(IDataProtectionProvider dataProtectionProvider)
        {
            this.dataProtectionProvider = dataProtectionProvider;
        }

        public string Encrypt(string purpose, string input)
        {
            return dataProtectionProvider.CreateProtector(purpose).Protect(input);
        }

        public string Decrypt(string purpose, string input)
        {
            return dataProtectionProvider.CreateProtector(purpose).Unprotect(input);
        }
    }
}
