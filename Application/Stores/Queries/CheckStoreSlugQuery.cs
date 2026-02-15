using Application.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Stores.Queries;

public class CheckStoreSlugQuery : IRequest<CheckStoreSlugResult>
{
	public string Slug { get; set; } = string.Empty;
	public Guid? ExcludeId { get; set; }
}

public class CheckStoreSlugResult
{
	public bool Available { get; set; }
	public string? Message { get; set; }
}

public class CheckStoreSlugHandler(
	IRepository<Store> repoStore
) : IRequestHandler<CheckStoreSlugQuery, CheckStoreSlugResult>
{
	public async Task<CheckStoreSlugResult> Handle(CheckStoreSlugQuery req, CancellationToken ct)
	{
		if (string.IsNullOrWhiteSpace(req.Slug))
			return new CheckStoreSlugResult { Available = false, Message = "Slug boş olamaz." };

		var query = repoStore.Query()
			.Where(s => s.Slug == req.Slug);

		if (req.ExcludeId.HasValue)
			query = query.Where(s => s.Id != req.ExcludeId.Value);

		var exists = await query.AnyAsync(ct);

		return new CheckStoreSlugResult
		{
			Available = !exists,
			Message = exists ? "Bu slug zaten kullanılmakta." : "Bu slug kullanılabilir."
		};
	}
}
