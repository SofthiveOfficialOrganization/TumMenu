using Application.BlogPosts.DTOs;
using Domain.Entities;
using Mapster;

namespace Application.BlogPosts;

public class BlogPostMappingProfiles : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<BlogPost, BlogPostDTO>();
        config.NewConfig<BlogPost, BlogPostListDTO>();
    }
}
