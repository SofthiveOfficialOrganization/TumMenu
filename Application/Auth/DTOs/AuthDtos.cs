namespace Application.Auth.DTOs;

public sealed record AuthResultDto(string AccessToken, DateTime ExpiresAt, string RefreshToken);
public sealed record LoginRequest(string Email, string Password);
public sealed record RegisterOwnerRequest(string Email, string Password, string FirstName, string LastName);
