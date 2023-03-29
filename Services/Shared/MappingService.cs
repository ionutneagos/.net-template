using Contracts.Catalog.Request;
using Domain.Entities.Catalog;
using Mapster;

namespace Services.Shared
{
    public class MappingService : IMappingService
    {
        private TypeAdapterConfig appUserMappings;
        public TypeAdapterConfig GetAppUserMappings() => appUserMappings;

        public MappingService()
        {
            InitAppUserMappings();
        }

        private void InitAppUserMappings()
        {
            appUserMappings = new TypeAdapterConfig();
            appUserMappings.NewConfig<CreateUserRequest, ApplicationUser>()
               .Map(dest => dest.UserName, src => src.Email)
               .Map(dest=> dest.CreatedDate, src => DateTime.UtcNow );
        }
    }
}
