using Contracts.Catalog;
using Domain.Constants;
using Mapster;
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
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await serviceManager.SampleService.GetAllAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await serviceManager.SampleService.GetByIdAsync(id);
            result.Name = serviceManager.DataEncryptionService.Decrypt(DataProtectorPurpose.GenericPurpose, result.Name);
            return Ok(result.Adapt<SampleDto>());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SampleForCreationDto sampleForCreationDto, CancellationToken cancellationToken)
        {
            sampleForCreationDto.Name = serviceManager.DataEncryptionService.Encrypt(DataProtectorPurpose.GenericPurpose, sampleForCreationDto.Name);
            var result = await serviceManager.SampleService.AddSampleAsync(sampleForCreationDto, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SampleForCreationDto sampleForCreationDto, CancellationToken cancellationToken)
        {
            await serviceManager.SampleService.UpdateSampleAsync(id, sampleForCreationDto, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await serviceManager.SampleService.DeleteAsync(id, cancellationToken);

            return NoContent();
        }
    }
}
