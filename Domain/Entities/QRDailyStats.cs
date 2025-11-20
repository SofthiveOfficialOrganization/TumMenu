using Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
