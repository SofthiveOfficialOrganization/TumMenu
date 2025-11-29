using Application.Abstractions;
using Application.Common.Exceptions;
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
	IMapper mapper
) : IRequestHandler<GetStoreByIdQuery, StoreDTO>
{
	public async Task<StoreDTO> Handle(GetStoreByIdQuery req, CancellationToken ct)
	{
		var store = await repoStore.Query()
			.Include(s => s.Company)
			.Include(s => s.Address)
			.Include(s => s.Staffs)
			.Include(s => s.Menus)
			.FirstOrDefaultAsync(s => s.Id == req.Id, ct)
			?? throw new NotFoundAppException("Dükkan bulunamadı.");
		var storeDTO = mapper.Map<StoreDTO>(store);
		return storeDTO;
	}
}