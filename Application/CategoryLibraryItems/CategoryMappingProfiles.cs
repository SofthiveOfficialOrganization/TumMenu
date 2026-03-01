using Application.Categories.Commands;
using Application.Categories.DTOs;
using Domain.Entities;
using Mapster;

namespace Application.Categories;

public class CategoryMappingProfiles : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CategoryLibraryItem, CategoryLibraryItemDTO>();
        config.NewConfig<Category, CategoryDTO>();
        
        config.NewConfig<Category, CategoryListDTO>()
            .Map(dest => dest.Title, src => src.CategoryLibraryItem != null ? src.CategoryLibraryItem.Title : null)
            .Map(dest => dest.Slug, src => src.CategoryLibraryItem != null ? src.CategoryLibraryItem.Slug : null)
            .Map(dest => dest.Description, src => src.CategoryLibraryItem != null ? src.CategoryLibraryItem.Description : null)
            .Map(dest => dest.IconKey, src => src.CategoryLibraryItem != null ? src.CategoryLibraryItem.IconKey : null)
            .Map(dest => dest.MenuName, src => src.Menu != null ? src.Menu.Title : null)
            .Map(dest => dest.CompanyId, src => src.Menu != null ? src.Menu.CompanyId : Guid.Empty)
            .Map(dest => dest.CompanyName, src => src.Menu != null && src.Menu.Company != null ? src.Menu.Company.Title : null)
            .Map(dest => dest.StoreId, src => src.Menu != null ? src.Menu.StoreId : (Guid?)null)
            .Map(dest => dest.StoreName, src => src.Menu != null && src.Menu.Store != null ? src.Menu.Store.Title : null)
            .Map(dest => dest.ProductCount, src => src.Products != null ? src.Products.Count : 0);
        
        config.NewConfig<CreateCategoryLibraryItemCommand, CategoryLibraryItem>();
        config.NewConfig<UpdateCategoryLibraryItemCommand, CategoryLibraryItem>();
    }
}
