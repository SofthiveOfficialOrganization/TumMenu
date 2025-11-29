namespace Application.Auths.DTOs;

public sealed class AuthResultDTO
{
	public string AccessToken { get; set; } = null!;
	public DateTime ExpiresAt { get; set; }
	public string RefreshToken { get; set; } = null!;
}

public sealed class SessionDTO
{
	public string UserId { get; set; } = null!;
	public string? Email { get; set; }
	public IReadOnlyList<string> Roles { get; set; } = null!;
	public Guid? OwnerId { get; set; }
	public Guid? CompanyId { get; set; }
	public string? CompanyName { get; set; }
}

public sealed class ApplicationUserLiteDTO
{
	public string Id { get; set; } = null!;
	public string UserName { get; set; } = null!;
	public string Email { get; set; } = null!;
}


