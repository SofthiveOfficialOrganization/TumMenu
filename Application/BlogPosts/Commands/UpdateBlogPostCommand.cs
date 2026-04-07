using Application.Abstractions;
using Application.Common.Helpers;
using Domain.Entities;
using Domain.Helpers;
using MediatR;

namespace Application.BlogPosts.Commands;

public class UpdateBlogPostCommand : IRequest<Guid>, ITransactionalRequest
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string Summary { get; set; } = null!;
    public string? CoverImageUrl { get; set; }
    public bool IsPublished { get; set; }
    public string? Tags { get; set; }
}

public class UpdateBlogPostHandler(
    IRepository<BlogPost> repo
) : IRequestHandler<UpdateBlogPostCommand, Guid>
{
    public async Task<Guid> Handle(UpdateBlogPostCommand req, CancellationToken ct)
    {
        var post = (await repo.GetByIdAsync(req.Id, ct))
            .EnsureFound("Blog yazısı bulunamadı.");

        post!.Title = req.Title;
        post.Slug = SlugHelper.Slugify(req.Title);
        post.Content = req.Content;
        post.Summary = req.Summary;
        post.CoverImageUrl = req.CoverImageUrl;
        if (!post.IsPublished && req.IsPublished)
            post.PublishedAt = DateTime.UtcNow;
        post.IsPublished = req.IsPublished;
        post.Tags = req.Tags;
        post.Modified();

        repo.Update(post);
        return post.Id;
    }
}
