using Application.Abstractions;
using Application.Common.Exceptions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Menus.Commands;

public class SetDefaultCompanyMenuCommand : IRequest<bool>, ITransactionalRequest
{
	public Guid Id { get; set; }
}

public class SetDefaultCompanyMenuCommandHandler(
	IRepository<Menu> repoMenu,
	IRepository<Company> repoCompany
) : IRequestHandler<SetDefaultCompanyMenuCommand, bool>
{
	public async Task<bool> Handle(SetDefaultCompanyMenuCommand req, CancellationToken ct)
	{
		var menu = await repoMenu.Query(tracked: false)
			.FirstOrDefaultAsync(x => x.Id == req.Id, ct);

		if (menu is null)
			return false;

		if (!menu.CompanyId.HasValue || menu.StoreId.HasValue || menu.Status != MenuStatus.MainMenu)
			throw new UnprocessableAppException("Yalnızca şirkete bağlı ana menüler varsayılan yapılabilir.");

		var company = await repoCompany.Query(tracked: true)
			.FirstOrDefaultAsync(x => x.Id == menu.CompanyId.Value, ct);

		if (company is null)
			throw new NotFoundAppException("Şirket bulunamadı.");

		company.DefaultMainMenuId = menu.Id;
		return true;
	}
}
