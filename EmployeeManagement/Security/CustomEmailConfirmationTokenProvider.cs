using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EmployeeManagement.Security
{
    public class CustomEmailConfirmationTokenProvider<TUser> :
        DataProtectorTokenProvider<TUser> where TUser : class
    {
        public CustomEmailConfirmationTokenProvider(IDataProtectionProvider dataProtectorProvider,
                                                    IOptions<CustomEmailConfirmationTokenProviderOptions> options,
                                                    ILogger<DataProtectorTokenProvider<TUser>> logger)
            : base(dataProtectorProvider, (IOptions<DataProtectionTokenProviderOptions>)options, logger) { }
    }
}
