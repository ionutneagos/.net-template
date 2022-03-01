using Contracts.Catalog.Request;
using Domain.Entities.Catalog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Services.Abstractions;
using System.Text.Json;

namespace Presentation.Controllers
{
    public class AuthenticationController : BaseController
    {
        private readonly IServiceManager serviceManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<AuthenticationController> logger;
        public AuthenticationController(UserManager<ApplicationUser> userManager, ILogger<AuthenticationController> logger, IServiceManager serviceManager, IConfiguration configuration)
            : base(configuration)
        {
            this.serviceManager = serviceManager;
            this.userManager = userManager;
            this.logger = logger;
        }


        #region Register Users
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterUser request)
        {
            if (!ModelState.IsValid)
                return ReturnInvalidRequest(ModelState, logger, JsonSerializer.Serialize(request));

            return Ok();
        }
        #endregion

        #region Login
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginUser request)
        {
            if (!ModelState.IsValid)
                return ReturnInvalidRequest(ModelState, logger, JsonSerializer.Serialize(request));

            //var user = await userService.AuthenticateAsync(loginViewModel.EmailAddress, loginViewModel.Password);

            //if (user == null)
            //    return EntityNotFound(logger, JsonConvert.SerializeObject(loginViewModel), "Login/Password does not match, please try again");

            //return GenerateTokenAndReturnProfile(user);
            return Ok();
        }
        #endregion
    }
}
