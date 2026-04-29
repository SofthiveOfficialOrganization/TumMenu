using Application.Categories.Commands;
using Application.Categories.DTOs;
using Domain.Entities;
using Mapster;

namespace Application.Categories;

public class CategoryMappingProfiles : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Default config ensures CategoryLibraryItem is mapped fully if properties match.
        config.NewConfig<CategoryLibraryItem, CategoryLibraryItemDTO>();
        config.NewConfig<Category, CategoryDTO>()
            .Ignore(dest => dest.Parent);
        
        config.NewConfig<Category, CategoryListDTO>()
            .Map(dest => dest.MenuName, src => src.Menu.Title)
            .Map(dest => dest.MenuId, src => src.Menu.Id)
            .Map(dest => dest.StoreName, src => src.Menu.Store != null ? src.Menu.Store.Title : null)
            .Map(dest => dest.StoreId, src => src.Menu.StoreId)
            .Map(dest => dest.CompanyName, src => src.Menu.Company != null ? src.Menu.Company.Title : (src.Menu.Store != null && src.Menu.Store.Company != null ? src.Menu.Store.Company.Title : null))
            .Map(dest => dest.CompanyId, src => src.Menu.CompanyId ?? (src.Menu.Store != null ? src.Menu.Store.CompanyId : Guid.Empty))
            .Map(dest => dest.Title, src => src.CategoryLibraryItem.Title)
            .Map(dest => dest.Slug, src => src.CategoryLibraryItem.Slug)
            .Map(dest => dest.Description, src => src.CategoryLibraryItem.Description)
            .Map(dest => dest.IconKey, src => src.CategoryLibraryItem.IconKey)
            .Map(dest => dest.ProductCount, src => src.Products != null ? src.Products.Count : 0)
            .Map(dest => dest.ParentId, src => src.ParentId)
            .Map(dest => dest.ParentName, src => src.Parent != null ? src.Parent.CategoryLibraryItem.Title : null);
        
        config.NewConfig<CreateCategoryLibraryItemCommand, CategoryLibraryItem>();
        config.NewConfig<UpdateCategoryLibraryItemCommand, CategoryLibraryItem>();
    }
}
