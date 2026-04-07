using Application.Abstractions;
using Application.BlogPosts.DTOs;
using Application.Common.Helpers;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.BlogPosts.Queries;

public class GetBlogPostBySlugQuery : IRequest<BlogPostDTO>
{
    public string Slug { get; set; } = null!;
    public bool PublishedOnly { get; set; } = true;
}

public class GetBlogPostBySlugHandler(
    IRepository<BlogPost> repo,
    IMapper mapper
) : IRequestHandler<GetBlogPostBySlugQuery, BlogPostDTO>
{
    public async Task<BlogPostDTO> Handle(GetBlogPostBySlugQuery req, CancellationToken ct)
    {
        var post = (await repo.Query()
            .Where(p => p.Slug == req.Slug && (!req.PublishedOnly || p.IsPublished))
            .FirstOrDefaultAsync(ct))
            .EnsureFound("Blog yazısı bulunamadı.");

        return mapper.Map<BlogPostDTO>(post);
    }
}
