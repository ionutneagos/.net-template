using Contracts;
using Contracts.Catalog;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services.Abstractions;

namespace Presentation.Controllers
{
    public class SampleController : BaseController
    {
        private readonly IServiceManager serviceManager;
        private readonly ILogger logger;
        public SampleController(IServiceManager serviceManager)
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
            var query = serviceManager.SampleService.GetAll()
                        .Select(t => new SampleDto { Id = t.Id, Name = t.Name, CreatedDate = t.CreatedDate });

            return Ok(GetSearchResult<SampleDto>(Request, query));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SampleDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await serviceManager.SampleService.GetByIdAsync(id);
            if (result == null)
                return NotFound();
            return Ok(result.Adapt<SampleDto>());
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SampleDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create([FromBody] SampleForCreationDto sampleForCreationDto, CancellationToken cancellationToken)
        {
            var result = await serviceManager.SampleService.AddSampleAsync(sampleForCreationDto, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Update(Guid id, [FromBody] SampleForCreationDto sampleForCreationDto, CancellationToken cancellationToken)
        {
            await serviceManager.SampleService.UpdateSampleAsync(id, sampleForCreationDto, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await serviceManager.SampleService.DeleteAsync(id, cancellationToken);

            return NoContent();
        }
    }
}
