using Application.Categories.Commands;
using Application.Categories.DTOs;
using Domain.Entities;
using Domain.Helpers;
using Mapster;

namespace Application.Categories
{
	public class CategoryMapping
	{
		public void Register(TypeAdapterConfig config)
		{
			config.NewConfig<Category, CategoryDTO>();
			config.NewConfig<CreateCategoryCommand, Category>();
			config.NewConfig<UpdateCategoryCommand, Category>();
		}
	}
}
