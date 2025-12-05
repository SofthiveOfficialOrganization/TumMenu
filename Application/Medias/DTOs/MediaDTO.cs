using Application.Common.Base.DTOs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Medias.DTOs;

public sealed class MediaDTO : BaseDTO
{
	public string MediaLink { get; set; } = null!;
	public string? AltText { get; set; }
	public int SortOrder { get; set; }
	public Guid ReferenceId { get; set; }
	public MediaRefType Type { get; set; } = MediaRefType.Unknown;
	public string Slot { get; set; } = "default-gallery";
	public int? Width { get; set; }
	public int? Height { get; set; }
}
