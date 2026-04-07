using Application.Abstractions;
using Application.BlogPosts.DTOs;
using Application.Common.Helpers;
using Domain.Entities;
using MapsterMapper;
using MediatR;

namespace Application.BlogPosts.Queries;

public class GetBlogPostByIdQuery : IRequest<BlogPostDTO>
{
    public Guid Id { get; set; }
}

public class GetBlogPostByIdHandler(
    IRepository<BlogPost> repo,
    IMapper mapper
) : IRequestHandler<GetBlogPostByIdQuery, BlogPostDTO>
{
    public async Task<BlogPostDTO> Handle(GetBlogPostByIdQuery req, CancellationToken ct)
    {
        var post = (await repo.GetByIdAsync(req.Id, ct))
            .EnsureFound("Blog yazısı bulunamadı.");

        return mapper.Map<BlogPostDTO>(post);
    }
}
