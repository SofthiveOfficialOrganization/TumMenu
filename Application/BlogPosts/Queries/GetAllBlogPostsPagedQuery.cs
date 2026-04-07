using Application.Abstractions;
using Application.BlogPosts.DTOs;
using Application.Common.Base.DTOs;
using Application.Common.Base.Page.RequestBase;
using Domain.Entities;
using MapsterMapper;
using MediatR;

namespace Application.BlogPosts.Queries;

public class GetAllBlogPostsPagedQuery : PageRequest, IRequest<PaginatedListDTO<BlogPostListDTO>>
{
    public bool? IsPublished { get; set; }
}

public class GetAllBlogPostsPagedHandler(
    IRepository<BlogPost> repo,
    IMapper mapper
) : IRequestHandler<GetAllBlogPostsPagedQuery, PaginatedListDTO<BlogPostListDTO>>
{
    public async Task<PaginatedListDTO<BlogPostListDTO>> Handle(GetAllBlogPostsPagedQuery req, CancellationToken ct)
    {
        var posts = await repo.GetPageListAsync(
            req,
            p => (!req.IsPublished.HasValue || p.IsPublished == req.IsPublished.Value),
            orderBy: p => p.OrderByDescending(p => p.PublishedAt),
            ct: ct
        );
        return mapper.Map<PaginatedListDTO<BlogPostListDTO>>(posts);
    }
}
