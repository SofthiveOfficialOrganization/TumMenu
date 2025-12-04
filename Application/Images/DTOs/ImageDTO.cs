using Application.Common.Base.DTOs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Images.DTOs;

public sealed class ImageDTO : BaseDTO
{
	public string ImageLink { get; set; } = null!;
	public string? AltText { get; set; }
	public int SortOrder { get; set; }
	public Guid ReferenceId { get; set; }
	public ImageRefType Type { get; set; } = ImageRefType.Unknown;
	public string Slot { get; set; } = "default-gallery";
	public int? Width { get; set; }
	public int? Height { get; set; }
}
