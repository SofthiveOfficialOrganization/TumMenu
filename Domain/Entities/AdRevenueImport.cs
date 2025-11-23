using Domain.Base;

namespace Domain.Entities
{
	public class AdRevenueImport : BaseEntity
	{
		public string Network { get; set; } = "AdSense";
		public DateOnly Day { get; set; }
		public decimal Revenue { get; set; }
		public string Currency { get; set; } = "TRY";
		public string? SourceFile { get; set; } // report file
	}
}
