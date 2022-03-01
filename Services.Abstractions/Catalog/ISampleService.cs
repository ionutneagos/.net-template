using Contracts.Catalog;
using Domain.Entities.Catalog;

namespace Services.Abstractions.Catalog
{
    public interface ISampleService : IGenericService<Sample>
    {
        Task<IEnumerable<SampleDto>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<SampleDto> AddSampleAsync(SampleForCreationDto dto, CancellationToken cancellationToken = default);

        Task UpdateSampleAsync(Guid id, SampleForCreationDto dto, CancellationToken cancellationToken = default);
    }
}
