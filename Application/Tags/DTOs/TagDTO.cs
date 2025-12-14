using Application.Common.Base.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Tags.DTOs;

public sealed class TagDTO : BaseDTO
{
	public string Name { get; set; } = null!;
}