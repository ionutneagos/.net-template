using Contracts.Catalog.Request;
using Contracts.Catalog.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services.Abstractions.Shared;
using System.Text.Json;

namespace Presentation.Controllers.v1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    public class TokenController : BaseController
    {
        private readonly ITokenService tokenService;
        private readonly ILogger logger;

        public TokenController(ITokenService tokenService, ILogger<TokenController> logger)
            : base()
        {
            this.tokenService = tokenService;
            this.logger = logger;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Get([FromBody] LoginUser request, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return ReturnInvalidRequest(ModelState, logger, JsonSerializer.Serialize(request));

            TokenResponse? response = await tokenService.GenerateAccessTokenAsync(request, cancellationToken);
            if (response == null)
                return NotFound("User or password invalid");

            return Ok(response);
        }

        [Authorize]
        [HttpPost]
        [Route("refresh")]
        public async Task<IActionResult> Refresh([FromBody] Token request, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return ReturnInvalidRequest(ModelState, logger, JsonSerializer.Serialize(request));

            TokenResponse? response = await tokenService.RenewTokenAsync(request, cancellationToken);
            if (response == null)
                return NotFound("Token invalid");

            return Ok(response);
        }

        [Authorize]
        [HttpPost]
        [Route("revoke")]
        [HttpPost]
        public async Task<IActionResult> Revoke(CancellationToken cancellationToken = default)
        {
            await tokenService.RevokeTokenAsync(cancellationToken);
            return Ok();
        }
    }
}
