using Domain.Base;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
	public enum QRResolveMode { LatestActive = 1, StaticUrl = 2, StaticMenu = 3 }
	public enum QRECCLevel { L = 1, M = 2, Q = 3, H = 4 }
	public class QRCode : BaseEntity
	{
		[MaxLength(50)] public string PublicKey { get; set; } = null!; // /q/{key}
		public Guid? StoreId { get; set; }
		public Store? Store { get; set; } // Left nullable for future use cases

		public QRResolveMode ResolveMode { get; set; } = QRResolveMode.LatestActive;
		[MaxLength(2048)] public string? TargetUrl { get; set; } // if static url
		public Guid? MenuId { get; set; } // if static menu

		public bool IsActive { get; set; } = true;
		public QRECCLevel ECCLevel { get; set; } = QRECCLevel.M;
		public string? StyleJson { get; set; }
	}
}
