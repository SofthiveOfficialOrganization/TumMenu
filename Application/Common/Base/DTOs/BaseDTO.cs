namespace Application.Common.Base.DTOs;

public abstract class BaseDTO
{
	public Guid Id { get; init; }
	public DateTimeOffset? CreatedAt { get; init; }
	public DateTimeOffset? ModifiedAt { get; init; }
	public DateTimeOffset? DeletedAt { get; init; }
	public bool IsDeleted { get; init; }
	public DateTimeOffset? DeletedAt { get; init; }
}
