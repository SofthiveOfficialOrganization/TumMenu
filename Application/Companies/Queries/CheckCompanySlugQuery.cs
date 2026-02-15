using Application.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Companies.Queries;

public class CheckCompanySlugQuery : IRequest<CheckCompanySlugResult>
{
	public string Slug { get; set; } = string.Empty;
	public Guid? ExcludeId { get; set; }
}

public class CheckCompanySlugResult
{
	public bool Available { get; set; }
	public string? Message { get; set; }
}

public class CheckCompanySlugHandler(
	IRepository<Company> repoCompany
) : IRequestHandler<CheckCompanySlugQuery, CheckCompanySlugResult>
{
	public async Task<CheckCompanySlugResult> Handle(CheckCompanySlugQuery req, CancellationToken ct)
	{
		if (string.IsNullOrWhiteSpace(req.Slug))
			return new CheckCompanySlugResult { Available = false, Message = "Slug boş olamaz." };

		var query = repoCompany.Query()
			.Where(c => c.Slug == req.Slug);

		if (req.ExcludeId.HasValue)
			query = query.Where(c => c.Id != req.ExcludeId.Value);

		var exists = await query.AnyAsync(ct);

		return new CheckCompanySlugResult
		{
			Available = !exists,
			Message = exists ? "Bu slug zaten kullanılmakta." : "Bu slug kullanılabilir."
		};
	}
}
