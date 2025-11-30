using Application.Common.Base.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Products.DTOs;

public sealed class ProductDTO() : BaseDTO
{
	public string Name { get; set; } = null!;
	public string? Description { get; set; }
	public Guid? CategoryId { get; set; }
	public decimal BasePrice { get; set; }
	public int SortOrder { get; set; }
	public bool IsActive { get; set; }
	public string? Allergens { get; set; }
	public bool? IsVegan { get; set; }
	public bool? IsVegetarian { get; set; }
	public int? EstimatedPreparationTimeInMinutes { get; set; }
}