using Application.Abstractions;
using Domain.Entities;
using Domain.Helpers;
using MediatR;

namespace Application.BlogPosts.Commands;

public class CreateBlogPostCommand : IRequest<Guid>, ITransactionalRequest
{
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string Summary { get; set; } = null!;
    public string? CoverImageUrl { get; set; }
    public bool IsPublished { get; set; }
    public string? Tags { get; set; }
}

public class CreateBlogPostHandler(
    IRepository<BlogPost> repo
) : IRequestHandler<CreateBlogPostCommand, Guid>
{
    public async Task<Guid> Handle(CreateBlogPostCommand req, CancellationToken ct)
    {
        var post = new BlogPost
        {
            Id = Guid.NewGuid(),
            Title = req.Title,
            Slug = SlugHelper.Slugify(req.Title),
            Content = req.Content,
            Summary = req.Summary,
            CoverImageUrl = req.CoverImageUrl,
            IsPublished = req.IsPublished,
            PublishedAt = req.IsPublished ? DateTime.UtcNow : default,
            Tags = req.Tags
        };
        post.Created();
        await repo.AddAsync(post, ct);
        return post.Id;
    }
}
