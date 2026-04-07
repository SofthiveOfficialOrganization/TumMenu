using Application.BlogPosts.Commands;
using Application.BlogPosts.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "OwnerOrAdmin")]
public class BlogController(IMediator mediator) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, CancellationToken ct = default)
    {
        var posts = await mediator.Send(new GetAllBlogPostsPagedQuery
        {
            PageIndex = page,
            PageSize = 20
        }, ct);
        return View(posts);
    }

    [HttpGet]
    public IActionResult Create() => View(new CreateBlogPostCommand());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBlogPostCommand cmd, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(cmd);
        await mediator.Send(cmd, ct);
        TempData["Success"] = "Yazı oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var post = await mediator.Send(new GetBlogPostByIdQuery { Id = id }, ct);
        var cmd = new UpdateBlogPostCommand
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            Summary = post.Summary,
            CoverImageUrl = post.CoverImageUrl,
            IsPublished = post.IsPublished,
            Tags = post.Tags
        };
        return View(cmd);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateBlogPostCommand cmd, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(cmd);
        await mediator.Send(cmd, ct);
        TempData["Success"] = "Yazı güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteBlogPostCommand { Id = id }, ct);
        TempData["Success"] = "Yazı silindi.";
        return RedirectToAction(nameof(Index));
    }
}
