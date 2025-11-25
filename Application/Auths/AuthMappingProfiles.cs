using Application.Auths.DTOs;
using Domain.Entities;
using Mapster;

namespace Application.Auths
{
    public class AuthMappingProfiles
    {
        public AuthMappingProfiles(TypeAdapterConfig config)
        {
            config.NewConfig<ApplicationUser, ApplicationUserLiteDTO>();
        }
    }
}
