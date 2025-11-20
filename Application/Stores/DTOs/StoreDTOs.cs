using Application.Common.Base.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Stores.DTOs
{
	public sealed record StoreLiteDTO(
		string Name,
		string Slug
	) : BaseDTO, ISluggableDTO;
}
