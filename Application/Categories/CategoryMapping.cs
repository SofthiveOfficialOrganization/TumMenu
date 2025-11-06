using Application.Categories.Commands;
using Domain.Entities;
using Mapster;
using System.Text;

namespace Application.Categories
{
	public class CategoryMapping
	{
		public void Register(TypeAdapterConfig config)
		{
			// Entity -> DTO
			config.NewConfig<Category, CategoryDto>();

			// Command -> Entity (Id/Slug dahil)
			config.NewConfig<CreateCategoryCommand, Category>()
				  .Map(d => d.Id, _ => Guid.NewGuid())
				  .Map(d => d.MenuId, s => s.MenuId)
				  .Map(d => d.Name, s => s.Name.Trim())
				  .Map(d => d.Slug, s => Slugify(s.Name))
				  .Map(d => d.SortOrder, s => s.SortOrder);
		}

		// Basit slug helper
		private static string Slugify(string text)
		{
			text = text.Trim().ToLowerInvariant();
			var normalized = text.Normalize(NormalizationForm.FormD);
			var sb = new StringBuilder(normalized.Length);
			foreach(var c in normalized)
			{
				var ch = c switch
				{
					'ğ' => 'g',
					'ü' => 'u',
					'ş' => 's',
					'ı' => 'i',
					'ö' => 'o',
					'ç' => 'c',
					'Ğ' => 'g',
					'Ü' => 'u',
					'Ş' => 's',
					'İ' => 'i',
					'I' => 'i',
					'Ö' => 'o',
					'Ç' => 'c',
					_ => char.ToLowerInvariant(c)
				};
				if(char.IsLetterOrDigit(ch)) sb.Append(ch);
				else if(char.IsWhiteSpace(ch) || ch == '-' || ch == '_') sb.Append('-');
			}
			var slug = sb.ToString().Trim('-');
			while(slug.Contains("--")) slug = slug.Replace("--", "-");
			return slug;
		}
	}
}
