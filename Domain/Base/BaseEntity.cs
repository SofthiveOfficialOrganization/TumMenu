using System.ComponentModel.DataAnnotations;

namespace Domain.Base
{
	public class BaseEntity : IBaseEntity
	{
		[Key]
		public Guid Id { get; set; }

		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? ModifiedAt { get; set; }
		public string? CreatedBy { get; set; }
		public string? ModifiedBy { get; set; }
		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }
		public void Created(string? userId = null)
		{
			if(CreatedAt != default) return;
			// TRT (UTC+3)
			CreatedAt = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3));
			CreatedBy = userId ?? string.Empty;
		}

		public void Modified(string? userId = null, bool isDeleted = false)
		{
			ModifiedAt = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3));
			ModifiedBy = userId;
			if(isDeleted) Deleted(userId);
		}

		public void Deleted(string? userId = null)
		{
			IsDeleted = true;
			DeletedAt = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3));
			ModifiedBy = userId;
			ModifiedAt = DeletedAt;
		}
	}
}
