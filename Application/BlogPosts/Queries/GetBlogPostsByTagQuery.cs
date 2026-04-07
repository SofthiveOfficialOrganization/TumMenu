using Application.Abstractions;
using Application.BlogPosts.DTOs;
using Application.Common.Base.DTOs;
using Application.Common.Base.Page.RequestBase;
using Domain.Entities;
using MapsterMapper;
using MediatR;

namespace Application.BlogPosts.Queries;

public class GetBlogPostsByTagQuery : PageRequest, IRequest<PaginatedListDTO<BlogPostListDTO>>
{
    public string Tag { get; set; } = null!;
}

public class GetBlogPostsByTagHandler(
    IRepository<BlogPost> repo,
    IMapper mapper
) : IRequestHandler<GetBlogPostsByTagQuery, PaginatedListDTO<BlogPostListDTO>>
{
    public async Task<PaginatedListDTO<BlogPostListDTO>> Handle(GetBlogPostsByTagQuery req, CancellationToken ct)
    {
        var posts = await repo.GetPageListAsync(
            req,
            p => p.IsPublished && p.Tags != null && p.Tags.Contains(req.Tag),
            orderBy: p => p.OrderByDescending(p => p.PublishedAt),
            ct: ct
        );
        return mapper.Map<PaginatedListDTO<BlogPostListDTO>>(posts);
    }
}
