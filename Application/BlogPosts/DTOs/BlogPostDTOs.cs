namespace Application.BlogPosts.DTOs;

public class BlogPostDTO
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string Summary { get; set; } = null!;
    public string? CoverImageUrl { get; set; }
    public DateTime PublishedAt { get; set; }
    public bool IsPublished { get; set; }
    public int ViewCount { get; set; }
    public string? Tags { get; set; }
}

public class BlogPostListDTO
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string Summary { get; set; } = null!;
    public string? CoverImageUrl { get; set; }
    public DateTime PublishedAt { get; set; }
    public bool IsPublished { get; set; }
    public string? Tags { get; set; }
}
