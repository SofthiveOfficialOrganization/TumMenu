using Application.Companies.Commands;
using Application.Companies.DTOs;
using Domain.Entities;
using Mapster;

namespace Application.Companies;

public class CompanyMappingProfiles
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Company, CompanyLiteDTO>();
        config.NewConfig<Company, CompanyDTO>().TwoWays();
        config.NewConfig<CreateCompanyCommand, Company>();
        config.NewConfig<UpdateCompanyCommand, Company>();
    }
}
