using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Base.DTOs;

public interface ISluggableDTO
{
	string Slug { get; init; }
}