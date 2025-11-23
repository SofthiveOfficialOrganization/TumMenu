using Domain.Base;

namespace Domain.Entities
{
	public class QRDailyStats : BaseEntity
	{
		public Guid QRCodeId { get; set; }
		public DateOnly Day { get; set; }
		public int Scans { get; set; }
		public int UniqueIps { get; set; }
	}
}
