using Contracts.Catalog;
using Domain.Entities.Catalog;
using Domain.Exceptions;
using Domain.Repositories;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Services.Abstractions;
using Services.Abstractions.Catalog;

namespace Services.Catalog
{
    internal sealed class SampleService : GenericService<Sample>, ISampleService
    {
        public SampleService(IServiceManager serviceManager, IGenericRepository<Sample> repository)
            : base(serviceManager, repository)
        {
        }

        public async Task<IEnumerable<SampleDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            List<Sample> items = await repository.GetAll().ToListAsync(cancellationToken);

            return items.Adapt<IEnumerable<SampleDto>>();
        }

        public async Task<SampleDto> AddSampleAsync(SampleForCreationDto dto, CancellationToken cancellationToken = default)
        {
            Sample sample = dto.Adapt<Sample>();
            await CreateAsync(sample, cancellationToken);
            return sample.Adapt<SampleDto>();
        }

        public async Task UpdateSampleAsync(Guid id, SampleForCreationDto dto, CancellationToken cancellationToken = default)
        {
            Sample? item = await GetByIdAsync(id);

            if (item is null)
                throw new NotFoundException("Entity not found");

            item.Name = dto.Name;

            await UpdateAsync(item, cancellationToken);
        }
    }
}
