using Application.Abstractions;
using Application.Menus.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;

namespace Application.Menus.Commands;

public record CreateMenuToCompanyCommand(
	string Name,
	Guid CompanyId
) : IRequest<MenuDTO>, ITransactionalRequest;

public class CreateMenuToCompanyHandler(
	IRepository<Menu> repoMenu,
	IMapper mapper
) : IRequestHandler<CreateMenuToCompanyCommand, MenuDTO>
{
	public async Task<MenuDTO> Handle(CreateMenuToCompanyCommand req, CancellationToken ct)
	{
		var menu = mapper.Map<Menu>(req);
		await repoMenu.AddAsync(menu, ct);
		var menuDTO = mapper.Map<MenuDTO>(menu);
		return menuDTO;
	}
}