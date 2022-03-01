using Domain.Entities.Catalog;
using Microsoft.AspNetCore.Identity;

namespace Persistence.CatalogContext.CatalogSeed
{
    public class CatalogDataSeeder
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<ApplicationRole> roleManager;

        private const string RootId = "root-id";
        private const string RootEmail = "root@root.ro";
        private const string RootPassword = "Secret.1";

        private const string RootRole = "Admin";
        private const string RootRoleId = "role-admin-id";
        private const string RootRoleTag = "seed-role";
        public CatalogDataSeeder(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public async Task SeedAsync()
        {
            await CreateDefaulUserAsync();
        }

        public async Task CreateDefaulUserAsync()
        {
            var rootUser = await userManager.FindByEmailAsync(RootEmail);
            var rootRole = await roleManager.FindByNameAsync(RootRole);
            IdentityResult result = new();

            if (rootUser == null)
            {
                rootUser = new ApplicationUser()
                {
                    Email = RootEmail,
                    UserName = RootEmail,
                    Id = RootId
                };
                result = await userManager.CreateAsync(rootUser);
                if (result.Succeeded)
                {
                    result = await userManager.AddPasswordAsync(rootUser, RootPassword);
                }
            }
            if (!result.Errors.Any())
            {
                if (rootRole == null)
                {
                    rootRole = new ApplicationRole { Name = RootRole, Id = RootRoleId, CustomTag = RootRoleTag };
                    result = await roleManager.CreateAsync(rootRole);
                }
            }
            if (!result.Errors.Any())
            {
                await userManager.AddToRoleAsync(rootUser, rootRole.Name);
            }
        }
    }
}
