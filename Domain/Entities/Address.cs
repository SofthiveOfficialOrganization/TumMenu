using Domain.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
	public class Address : BaseEntity
	{
		public double? Latitude { get; set; }
		public double? Longitude { get; set; }
		public Guid? CityId { get; set; }
		public Guid? DistrictId { get; set; }
		[MaxLength(200)] public string? Neighborhood { get; set; }
		public string? FullAddress { get; set; }
		public Guid StoreId { get; set; }
		public Store Store { get; set; } = null!;
	}
}
