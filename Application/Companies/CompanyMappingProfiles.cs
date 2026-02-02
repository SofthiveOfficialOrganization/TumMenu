using Application.Companies.Commands;
using Application.Companies.DTOs;
using Domain.Entities;
using Domain.Helpers;
using Mapster;

namespace Application.Companies;

public class CompanyMappingProfiles
{
	public void Register(TypeAdapterConfig config)
	{
		config.NewConfig<Company, CompanyLiteDTO>();
		config.NewConfig<Company, CompanyDTO>().TwoWays();
		config.NewConfig<CreateCompanyCommand, Company>()
			.Map(dest => dest.Slug,
				src => string.IsNullOrWhiteSpace(src.Slug)
					? SlugHelper.Slugify(src.Title)
					: src.Slug);
		config.NewConfig<UpdateCompanyCommand, Company>();
	}
}
