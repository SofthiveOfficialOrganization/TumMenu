using Application.Abstractions;
using Application.Common.Exceptions;
using Application.Menus.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;

namespace Application.Menus.Commands;

public class CopyMenuCommand : IRequest<MenuDTO>, ITransactionalRequest
{
	public Guid SourceMenuId { get; set; }
	public Guid TargetMenuId { get; set; }
}

public class CopyMenuHandler(
	IRepository<Menu> repoMenu,
	IMapper mapper
) : IRequestHandler<CopyMenuCommand, MenuDTO>
{
	public async Task<MenuDTO> Handle(CopyMenuCommand req, CancellationToken ct)
	{
		var sourceMenu = await repoMenu.GetByIdAsync(req.SourceMenuId, ct);
		if(sourceMenu is null)
			throw new NotFoundAppException("Kaynak menü bulunamadı, tekrardan menü oluşturun.");

		var targetMenu = await repoMenu.GetByIdAsync(req.TargetMenuId, ct);
		if(targetMenu is null)
			throw new NotFoundAppException("Hedef menü bulunamadı, tekrardan menü oluşturun.");

		mapper.Map(sourceMenu, targetMenu);
		repoMenu.Update(targetMenu);
		var targetMenuDTO = mapper.Map<MenuDTO>(targetMenu);
		return targetMenuDTO;
	}
}