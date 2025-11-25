using Application.Categories.Commands;
using Application.Categories.DTOs;
using Domain.Entities;
using Mapster;

namespace Application.Categories;

public class CategoryMappingProfiles
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Category, CategoryDTO>();
        config.NewConfig<CreateCategoryCommand, Category>();
        config.NewConfig<UpdateCategoryCommand, Category>();
    }
}
