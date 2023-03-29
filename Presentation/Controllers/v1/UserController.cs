using Contracts;
using Contracts.Catalog.AppUser.Response;
using Contracts.Catalog.Request;
using Domain.Constants;
using Domain.Entities.Catalog;
using Domain.Extensions;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services.Abstractions;
using System.Text.Json;

namespace Presentation.Controllers.v1
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [Authorize(Roles = IdentityConfiguration.RootRole + "," + IdentityConfiguration.TenantRole)]
    public class UserController : BaseController
    {
        private readonly IServiceManager serviceManager;
        private readonly ILogger logger;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<ApplicationRole> roleManager;
        public UserController(IServiceManager serviceManager, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
            : base()
        {
            this.serviceManager = serviceManager;
            this.userManager = userManager;
            this.roleManager = roleManager;
            logger = serviceManager.SetLoger<TenantController>();
        }

        #region Search
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResponse<UserResponse>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetAll([FromQuery] SearchRequest request)
        {
            IQueryable<UserResponse> query = serviceManager.AppUserService.GetAll()
                        .Include(t => t.Tenant)
                        .Select(t => new UserResponse
                        {
                            Id = t.Id,
                            Email = t.Email,
                            FirstName = t.FirstName,
                            LastName = t.LastName,
                            TenantId = t.TenantId,
                            TenantName = t.Tenant != null ? t.Tenant.Name : "",
                            CreatedDate = t.CreatedDate,
                            CustomTag = t.CustomTag,
                            PhoneNumber = t.PhoneNumber,
                            UserName = t.UserName
                        });
            return Ok(GetSearchResult<UserResponse>(Request, query));
        }
        #endregion

        #region Details
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(string id)
        {
            ApplicationUser? result = await userManager.FindByIdAsync(id);
            if (result == null)
                return NotFound();
            return Ok(result.Adapt<UserResponse>());
        }
        #endregion

        #region  Create User For Tenants
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return ReturnInvalidRequest(ModelState, logger, JsonSerializer.Serialize(request));

            ApplicationUser user = request.Adapt<ApplicationUser>(serviceManager.MappingService.GetAppUserMappings());

            user.TenantId = User.GetTenantFromClaim();

            IdentityResult result = await userManager.CreateAsync(user);

            if (!result.Succeeded)
                return ReturnBadRequest(logger, JsonSerializer.Serialize(request), "User could not be added", result.Errors.Select(t => t.Description).ToList());

            UserResponse response = user.Adapt<UserResponse>();
            return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateWithPassword([FromBody] CreateUserWithPasswordRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return ReturnInvalidRequest(ModelState, logger, JsonSerializer.Serialize(request));

            ApplicationUser user = request.Adapt<ApplicationUser>(serviceManager.MappingService.GetAppUserMappings());
            user.TenantId = User.GetTenantFromClaim();

            IdentityResult result = new();

            result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                return ReturnBadRequest(logger, JsonSerializer.Serialize(request), "User could not be added", result.Errors.Select(t => t.Description).ToList());

            UserResponse response = user.Adapt<UserResponse>();
            return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
        }
        #endregion

        #region   User Tenant Admin User
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = IdentityConfiguration.RootRole)]
        public async Task<IActionResult> CreateTenantAdmin([FromBody] CreateAdminUserRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return ReturnInvalidRequest(ModelState, logger, JsonSerializer.Serialize(request));

            ApplicationUser user = request.Adapt<ApplicationUser>(serviceManager.MappingService.GetAppUserMappings());

            IdentityResult result = new();

            result = await EnsureTenantRoleExistAsync();

            if (!result.Succeeded)
                return ReturnBadRequest(logger, JsonSerializer.Serialize(request), "Tenant role could not be added", result.Errors.Select(t => t.Description).ToList());

            result = await userManager.CreateAsync(user);

            if (!result.Succeeded)
                return ReturnBadRequest(logger, JsonSerializer.Serialize(request), "User could not be added", result.Errors.Select(t => t.Description).ToList());

            result = await userManager.AddToRoleAsync(user, IdentityConfiguration.TenantRole);

            if (!result.Succeeded)
                return ReturnBadRequest(logger, JsonSerializer.Serialize(request), "User could not be added to tenant role", result.Errors.Select(t => t.Description).ToList());

            UserResponse response = user.Adapt<UserResponse>();
            return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = IdentityConfiguration.RootRole)]
        public async Task<IActionResult> CreateTenantAdminWithPassword([FromBody] CreateAdminUserWithPasswordRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return ReturnInvalidRequest(ModelState, logger, JsonSerializer.Serialize(request));

            ApplicationUser user = request.Adapt<ApplicationUser>(serviceManager.MappingService.GetAppUserMappings());

            IdentityResult result = new();

            result = await EnsureTenantRoleExistAsync();

            if (!result.Succeeded)
                return ReturnBadRequest(logger, JsonSerializer.Serialize(request), "Tenant role could not be added", result.Errors.Select(t => t.Description).ToList());

            result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                return ReturnBadRequest(logger, JsonSerializer.Serialize(request), "User could not be added", result.Errors.Select(t => t.Description).ToList());

            result = await userManager.AddToRoleAsync(user, IdentityConfiguration.TenantRole);

            if (!result.Succeeded)
                return ReturnBadRequest(logger, JsonSerializer.Serialize(request), "User could not be added to tenant role", result.Errors.Select(t => t.Description).ToList());

            UserResponse response = user.Adapt<UserResponse>();
            return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
        }
        #endregion

        #region Private Methods
        private async Task<IdentityResult> EnsureTenantRoleExistAsync()
        {
            return await EnsureRoleExistAsync(new ApplicationRole
            {
                Name = IdentityConfiguration.TenantRole,
                Id = IdentityConfiguration.TenantRoleId,
                CustomTag = IdentityConfiguration.TenantRoleTag
            });
        }

        private async Task<IdentityResult> EnsureRoleExistAsync(ApplicationRole applicationRole)
        {
            ApplicationRole? role = await roleManager.FindByNameAsync(roleName: applicationRole.Name);
            if (role == null)
                return await roleManager.CreateAsync(applicationRole);

            return IdentityResult.Success;
        }
        #endregion
    }
}
