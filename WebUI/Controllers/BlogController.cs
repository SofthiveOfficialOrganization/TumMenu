using Application.BlogPosts.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers;

public class BlogController(IMediator mediator) : Controller
{
    [HttpGet("/blog")]
    public async Task<IActionResult> Index(int page = 1, CancellationToken ct = default)
    {
        var posts = await mediator.Send(new GetAllBlogPostsPagedQuery
        {
            PageIndex = page,
            PageSize = 10,
            IsPublished = true
        }, ct);
        return View(posts);
    }

    [HttpGet("/blog/{slug}")]
    public async Task<IActionResult> Post(string slug, CancellationToken ct)
    {
        var post = await mediator.Send(new GetBlogPostBySlugQuery { Slug = slug }, ct);
        return View(post);
    }

    [HttpGet("/blog/etiket/{tag}")]
    public async Task<IActionResult> Tag(string tag, int page = 1, CancellationToken ct = default)
    {
        var posts = await mediator.Send(new GetBlogPostsByTagQuery
        {
            Tag = tag,
            PageIndex = page,
            PageSize = 10
        }, ct);
        ViewBag.Tag = tag;
        return View(posts);
    }
}
