using Contracts;
using Contracts.Catalog;
using Domain.Entities.Tenant;
using Domain.Exceptions;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services.Abstractions;
using System.Text.Json;

namespace Presentation.Controllers.v1
{
    /// <summary>
    /// 
    /// </summary>
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [Authorize]
    public class SampleTenantController : BaseController
    {
        private readonly IServiceManager serviceManager;
        private readonly ILogger logger;
        public SampleTenantController(IServiceManager serviceManager)
            : base()
        {
            this.serviceManager = serviceManager;
            logger = serviceManager.SetLoger<SampleController>();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResponse<SampleDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetAll([FromQuery] SearchRequest request)
        {
            IQueryable<SampleDto> query = serviceManager.SampleTenantService.GetAll()
                        .Select(t => new SampleDto { Id = t.Id, Name = t.Name, CreatedDate = t.CreatedDate });

            return Ok(GetSearchResult<SampleDto>(Request, query));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SampleDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(Guid id)
        {
            SampleTenant result = await serviceManager.SampleTenantService.GetByIdAsync(id);
            if (result == null)
                return NotFound();
            return Ok(result.Adapt<SampleDto>());
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SampleDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create([FromBody] SampleForCreationDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return ReturnInvalidRequest(ModelState, logger, JsonSerializer.Serialize(request));

            var item = request.Adapt<SampleTenant>();
            await serviceManager.SampleTenantService.CreateAsync(item, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Update(Guid id, [FromBody] SampleForCreationDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return ReturnInvalidRequest(ModelState, logger, JsonSerializer.Serialize(request));

            var item = await serviceManager.SampleTenantService.GetByIdAsync(id) ?? throw new NotFoundException("Entity not found");

            item = request.Adapt<SampleTenant>();

            await serviceManager.SampleTenantService.UpdateAsync(item, cancellationToken);

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await serviceManager.SampleTenantService.DeleteAsync(id, cancellationToken);

            return NoContent();
        }
    }
}
