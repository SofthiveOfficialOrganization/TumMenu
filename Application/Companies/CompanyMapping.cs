using Application.Companies.DTOs;
using Domain.Entities;
using Mapster;

namespace Application.Companies
{
    public class CompanyMapping
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Company, CompanyLiteDTO>();
        }
    }
}
