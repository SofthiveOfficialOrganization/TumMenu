using Application.Abstractions;
using Application.Common.Base.DTOs;
using Application.Common.Base.Page;
using Application.Common.Base.Page.RequestBase;
using Application.Tags.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Tags.Queries;

public sealed class GetTagsPagedQuery : PageRequest, IRequest<PaginatedListDTO<TagDTO>>
{
	public string? Search { get; set; }
}

public class GetTagsPagedHandler(
	IRepository<Tag> repoTag,
	IMapper mapper
) : IRequestHandler<GetTagsPagedQuery, PaginatedListDTO<TagDTO>>
{
	public async Task<PaginatedListDTO<TagDTO>> Handle(GetTagsPagedQuery req, CancellationToken ct)
	{
		var tags = await repoTag.GetPageListAsync(
			req,
			tag =>
				string.IsNullOrEmpty(req.Search) || (tag.Title != null && tag.Title.Contains(req.Search)),
			orderBy: tag => tag.OrderBy(t => t.Title),
			ct: ct
		);
		var tagsDTO = mapper.Map<PaginatedListDTO<TagDTO>>(tags);
		return tagsDTO;
	}
}