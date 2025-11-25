using Application.Abstractions;
using Application.Common.Exceptions;
using Domain.Entities;
using MapsterMapper;
using MediatR;

namespace Application.Companies.Commands;

public record DeleteCompanyCommand(Guid Id) : IRequest<Unit>, ITransactionalRequest;

public class DeleteCompanyCommandHandler(
	IRepository<Company> repoCompany,
	IMapper mapper
) : IRequestHandler<DeleteCompanyCommand, Unit>
{
	public async Task<Unit> Handle(DeleteCompanyCommand req, CancellationToken ct)
	{
		var company = await repoCompany.GetByIdAsync(req.Id, ct);
		if(company == null)
			throw new NotFoundAppException("Şirket bulunamadı");
		repoCompany.SoftDelete(company);
		return Unit.Value;
	}
}