using Contracts;
using Contracts.Catalog.AppTenant.Request;
using Contracts.Catalog.AppTenant.Response;
using Domain.Constants;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services.Abstractions;

namespace Presentation.Controllers.v1
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [Authorize(Roles = IdentityConfiguration.RootRole)]
    public class TenantController : BaseController
    {
        private readonly IServiceManager serviceManager;
        private readonly ILogger logger;
        public TenantController(IServiceManager serviceManager)
            : base()
        {
            this.serviceManager = serviceManager;
            logger = serviceManager.SetLoger<TenantController>();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResponse<TenantResponse>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetAll([FromQuery] SearchRequest request)
        {
            IQueryable<TenantResponse> query = serviceManager.AppTenantService.GetAll()
                        .Select(t => new TenantResponse { Id = t.Id, Name = t.Name, CreatedDate = t.CreatedDate });

            return Ok(GetSearchResult<TenantResponse>(Request, query));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TenantResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(int id)
        {
            Domain.Entities.Catalog.AppTenant result = await serviceManager.AppTenantService.GetByIdAsync(id);
            if (result == null)
                return NotFound();

            return Ok(result.Adapt<TenantResponse>());
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(TenantResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create([FromBody] CreateTenantRequest createRequest, CancellationToken cancellationToken)
        {
            TenantResponse result = await serviceManager.AppTenantService.AddAsync(createRequest, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }
    }
}