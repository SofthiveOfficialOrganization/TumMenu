namespace Application.Abstractions;

public interface IUserContext
{
    bool IsAuthenticated { get; }
    string? UserId { get; }
    string? UserName { get; }
    string? Email { get; }
    string? OwnerId { get; }
    string? CompanyId { get; }
    string? CompanyName { get; }
    IReadOnlyList<string> Roles { get; }
    string? RemoteIp { get; }
}