using Application.Abstractions;
using Application.Common.Exceptions;
using Application.Common.Helpers;
using Application.Microservices.Location;
using Application.Stores.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Stores.Queries;

public sealed record GetStoreByIdQuery
(
	Guid Id
) : IRequest<StoreDTO>;

public class GetStoreByIdQueryHandler(
	IRepository<Store> repoStore,
	IMapper mapper,
	ILocationMicroservice locationMicroservice
) : IRequestHandler<GetStoreByIdQuery, StoreDTO>
{
	public async Task<StoreDTO> Handle(GetStoreByIdQuery req, CancellationToken ct)
	{
		var store = (await repoStore.Query(tracked: false)
			.AsSplitQuery()
			.Include(s => s.Company)
			.Include(s => s.Address)
			.Include(s => s.Staffs)
			.Include(s => s.Medias)
			.Include(s => s.Menus)
			.FirstOrDefaultAsync(s => s.Id == req.Id, ct)).EnsureFound("Dükkan bulunamadı.");

		var storeDTO = mapper.Map<StoreDTO>(store!);

		// Enrich address with city/district names from location microservice
		if (storeDTO.Address != null)
		{
			var cityId = store!.Address?.CityId;
			var districtId = store!.Address?.DistrictId;

			if (cityId.HasValue)
			{
				// Run both calls in parallel
				var provinceTask = locationMicroservice.GetProvinceBasicByIdAsync(cityId.Value);
				var districtsTask = districtId.HasValue
					? locationMicroservice.GetDistrictsByProvinceIdAsync(cityId.Value)
					: Task.FromResult<List<Application.Microservices.Location.DTOs.GetDisctrictsResponseDTO>>([]);

				await Task.WhenAll(provinceTask, districtsTask);

				var province = await provinceTask;
				var districts = await districtsTask;

				storeDTO.Address.CityName = province?.Name;

				if (districtId.HasValue)
				{
					storeDTO.Address.DistrictName = districts
						.FirstOrDefault(d => d.Id == districtId.Value)?.Name;
				}
			}
		}

		return storeDTO;
	}
}