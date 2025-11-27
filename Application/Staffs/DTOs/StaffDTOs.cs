using Application.Auths.DTOs;
using Application.Common.Base.DTOs;

namespace Application.Staffs.DTOs;

public sealed class StaffDTO : BaseDTO
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = null!;
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = null!;
}

public sealed class StaffLiteDTO : BaseDTO
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = null!;
}