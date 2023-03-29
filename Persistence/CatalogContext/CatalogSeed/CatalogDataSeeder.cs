using Domain.Constants;
using Domain.Entities.Catalog;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Persistence.CatalogContext.CatalogSeed
{
    public class CatalogDataSeeder
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly IConfiguration configuration;

        public CatalogDataSeeder(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.configuration = configuration;
        }

        public async Task SeedAsync()
        {
            await CreateDefaulUserAsync();
        }

        public async Task CreateDefaulUserAsync()
        {
            string rootEmail = configuration["RootUser:Email"] ?? IdentityConfiguration.RootEmail;

            ApplicationUser? rootUser = await userManager.FindByEmailAsync(rootEmail);
            ApplicationRole? rootRole = await roleManager.FindByNameAsync(IdentityConfiguration.RootRole);

            IdentityResult result = new();

            if (rootUser == null)
            {
                rootUser = new ApplicationUser()
                {
                    Email = rootEmail,
                    UserName = rootEmail,
                    Id = IdentityConfiguration.RootId
                };
                result = await userManager.CreateAsync(rootUser, configuration["RootUser:Password"] ?? IdentityConfiguration.RootPassword);
            }

            if (!result.Errors.Any())
            {
                if (rootRole == null)
                {
                    rootRole = new ApplicationRole { Name = IdentityConfiguration.RootRole, Id = IdentityConfiguration.RootRoleId, CustomTag = IdentityConfiguration.RootRoleTag };
                    result = await roleManager.CreateAsync(rootRole);
                }
                if (!result.Errors.Any() && !string.IsNullOrEmpty(rootRole.Name))
                {
                    await userManager.AddToRoleAsync(rootUser, rootRole.Name);
                }
            }
        }
    }
}
