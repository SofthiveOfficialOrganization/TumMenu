namespace Domain.Base;

public interface IAuditable
{
	long CreatedOn { get; set; }
	string? CreatedBy { get; set; }
	long? ModifiedOn { get; set; }
	string? ModifiedBy { get; set; }
	bool IsDeleted { get; set; }
}
