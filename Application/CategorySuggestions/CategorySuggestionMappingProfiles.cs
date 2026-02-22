using Application.CategorySuggestions.DTOs;
using Domain.Entities;
using Mapster;

namespace Application.CategorySuggestions;

public class CategorySuggestionMappingProfiles
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CategorySuggestion, CategorySuggestionDTO>()
            .Map(dest => dest.CompanyName, src => src.Company != null ? src.Company.Title : src.SubmittedByName)
            .Map(dest => dest.Status, src => (int)src.Status);
    }
}
