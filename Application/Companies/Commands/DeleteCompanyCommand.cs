using Application.Abstractions;
using Application.Common.Exceptions;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Companies.Commands;

public class DeleteCompanyCommand : IRequest<Unit>, ITransactionalRequest, IEntityAuditableCommand
{
	public Guid Id { get; set; }
	public string ActionName => "Şirket silindi";
	public Guid EntityId => Id;
}

public class DeleteCompanyCommandHandler(
	IRepository<Company> repoCompany,
	IRepository<Store> repoStore,
	IRepository<Menu> repoMenu
) : IRequestHandler<DeleteCompanyCommand, Unit>
{
	public async Task<Unit> Handle(DeleteCompanyCommand req, CancellationToken ct)
	{
		var company = await repoCompany.GetByIdAsync(req.Id, ct);
		if(company == null)
			throw new NotFoundAppException("Şirket bulunamadı");

		// Soft delete company
		repoCompany.SoftDelete(company);

		// Soft delete associated stores to release slugs
		var stores = await repoStore.Query(tracked: true)
			.Where(s => s.CompanyId == company.Id)
			.ToListAsync(ct);
			
		foreach (var store in stores)
		{
			repoStore.SoftDelete(store);
		}

		// Soft delete associated company menus and store menus
		var menus = await repoMenu.Query(tracked: true)
			.Where(m => m.CompanyId == company.Id)
			.ToListAsync(ct);

		foreach (var menu in menus)
		{
			repoMenu.SoftDelete(menu);
		}
		
		return Unit.Value;
	}
}
