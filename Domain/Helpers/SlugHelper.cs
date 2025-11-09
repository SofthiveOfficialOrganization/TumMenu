using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Helpers
{
	public static class SlugHelper
	{
		public static string Slugify(string text)
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
