namespace Application.Common.Base.DTOs;

public abstract record BaseDTO
{
    public Guid Id { get; init; }
    public DateTime? CreatedOnValue { get; init; }
    public DateTime? ModifiedOnValue { get; init; }
    public bool IsDeleted { get; init; }
}