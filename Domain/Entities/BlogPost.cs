using Domain.Base;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class BlogPost : BaseEntity
{
    [MaxLength(300)] public string Title { get; set; } = null!;
    [MaxLength(300)] public string Slug { get; set; } = null!;
    public string Content { get; set; } = null!;
    [MaxLength(500)] public string Summary { get; set; } = null!;
    [MaxLength(500)] public string? CoverImageUrl { get; set; }
    public DateTime PublishedAt { get; set; }
    public bool IsPublished { get; set; }
    public int ViewCount { get; set; }
    [MaxLength(500)] public string? Tags { get; set; }
}
