using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Web.Providers
{
    public class AppTokenProvider<TUser> : DataProtectorTokenProvider<TUser> where TUser : class
    {
        public AppTokenProvider(IDataProtectionProvider dataProtectionProvider,
            IOptions<EmailConfirmationTokenProviderOptions> options,
            ILogger<DataProtectorTokenProvider<TUser>> logger)
                                              : base(dataProtectionProvider, options, logger)
        {

        }
    }

    public class EmailConfirmationTokenProviderOptions : DataProtectionTokenProviderOptions
    {
        public EmailConfirmationTokenProviderOptions()
        {
            Name = nameof(EmailConfirmationTokenProviderOptions);
            TokenLifespan = TimeSpan.FromHours(1);
        }
    }
}
