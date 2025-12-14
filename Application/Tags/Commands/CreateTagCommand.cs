using Application.Abstractions;
using Application.Common.Exceptions;
using Application.Common.Helpers;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Tags.Commands;

public sealed record CreateTagCommand(
	string Name
) : IRequest<Guid>, ITransactionalRequest;

public class CreateTagCommandHandler(
	IRepository<Tag> repoTag,
	IMapper mapper
) : IRequestHandler<CreateTagCommand, Guid>
{
	public async Task<Guid> Handle(CreateTagCommand req, CancellationToken ct)
	{
		var exists = await repoTag.ExistsAsync(t => t.Name == req.Name, ct);
		if(exists)
			throw new AlreadyExistsAppException("Verilen isimde bir etiket zaten mevcut.");
		var tag = mapper.Map<Tag>(req);
		await repoTag.AddAsync(tag, ct);
		return tag.Id;
	}
}