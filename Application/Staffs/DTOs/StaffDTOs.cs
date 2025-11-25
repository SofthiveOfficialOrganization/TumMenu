using Application.Auths.DTOs;
using Application.Common.Base.DTOs;

namespace Application.Staffs.DTOs;

public sealed record StaffDTO(
	string FirstName,
	string LastName,
	string? Email,
	string? PhoneNumber,
	string Role,
	Guid StoreId,
	string StoreName
) : BaseDTO;

public sealed record StaffLiteDTO(
	string FirstName,
	string LastName,
	string? Email,
	string? PhoneNumber,
	string Role
) : BaseDTO;