using Application.Auths.DTOs;
using Domain.Entities;
using Mapster;

namespace Application.Auths
{
    public class AuthMapper
    {
        public AuthMapper(TypeAdapterConfig config)
        {
            config.NewConfig<ApplicationUser, ApplicationUserLiteDTO>();
        }
    }
}
