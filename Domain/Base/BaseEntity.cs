using System.ComponentModel.DataAnnotations;

namespace Domain.Base
{
	public class BaseEntity : IBaseEntity
	{
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();

		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? ModifiedAt { get; set; }
		public string? CreatedBy { get; set; }
		public string? ModifiedBy { get; set; }
		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }
		public void Created(string? userId = null)
		{
			if(CreatedAt != default) return;
			CreatedAt = DateTimeOffset.UtcNow;
			CreatedBy = userId ?? string.Empty;
		}

		public void Modified(string? userId = null, bool isDeleted = false)
		{
			ModifiedAt = DateTimeOffset.UtcNow;
			ModifiedBy = userId;
			if(isDeleted) Deleted(userId);
		}

		public void Deleted(string? userId = null)
		{
			IsDeleted = true;
			DeletedAt = DateTimeOffset.UtcNow;
			ModifiedBy = userId;
			ModifiedAt = DeletedAt;
		}
	}
}
