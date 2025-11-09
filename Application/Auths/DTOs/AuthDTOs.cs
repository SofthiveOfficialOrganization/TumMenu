namespace Application.Auths.DTOs;

public sealed record AuthResultDTO(string AccessToken, DateTime ExpiresAt, string RefreshToken);
public sealed record SessionDTO(
	string UserId,
	string? Email,
	IReadOnlyList<string> Roles,
	Guid? OwnerId,
	Guid? StaffId,
	Guid? CompanyId,
	string? CompanyName
);
public sealed record BasicApplicationUserDTO(string Id, string UserName, string Email);