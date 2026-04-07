using Application.Abstractions;
using Application.Common.Helpers;
using Domain.Entities;
using MediatR;

namespace Application.BlogPosts.Commands;

public class DeleteBlogPostCommand : IRequest<Unit>, ITransactionalRequest
{
    public Guid Id { get; set; }
}

public class DeleteBlogPostHandler(
    IRepository<BlogPost> repo
) : IRequestHandler<DeleteBlogPostCommand, Unit>
{
    public async Task<Unit> Handle(DeleteBlogPostCommand req, CancellationToken ct)
    {
        var post = (await repo.GetByIdAsync(req.Id, ct))
            .EnsureFound("Blog yazısı bulunamadı.");

        repo.SoftDelete(post!);
        return Unit.Value;
    }
}
