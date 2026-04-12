using Application.Abstractions;
using Application.Common.Exceptions;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Menus.Commands;

public class DeleteMenuCommand : IRequest<Unit>, ITransactionalRequest, IEntityAuditableCommand
{
	public Guid Id { get; set; }
	public string ActionName => "Menü silindi";
	public Guid EntityId => Id;
}

public class DeleteMenuCommandHandler(
	IRepository<Menu> repoMenu,
	IRepository<Company> repoCompany,
	IMapper mapper
) : IRequestHandler<DeleteMenuCommand, Unit>
{
	public async Task<Unit> Handle(DeleteMenuCommand req, CancellationToken ct)
	{
		var menu = await repoMenu.GetByIdAsync(req.Id, ct);
		if(menu == null)
			throw new NotFoundAppException("Menü bulunamadı");

		if (menu.CompanyId.HasValue)
		{
			var company = await repoCompany.Query(tracked: true)
				.FirstOrDefaultAsync(x => x.Id == menu.CompanyId.Value, ct);

			if (company != null && company.DefaultMainMenuId == menu.Id)
			{
				var nextDefaultMenuId = await repoMenu.Query(tracked: false)
					.Where(x =>
						x.CompanyId == menu.CompanyId &&
						x.Id != menu.Id &&
						x.StoreId == null &&
						x.Status == MenuStatus.MainMenu)
					.OrderBy(x => x.CreatedAt)
					.Select(x => (Guid?)x.Id)
					.FirstOrDefaultAsync(ct);

				company.DefaultMainMenuId = nextDefaultMenuId;
			}
		}

		repoMenu.SoftDelete(menu);
		return Unit.Value;
	}
}
