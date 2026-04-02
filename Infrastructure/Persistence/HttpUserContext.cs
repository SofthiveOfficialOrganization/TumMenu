using Application.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Infrastructure.Persistence;

public sealed class HttpUserContext(IHttpContextAccessor accessor) : IUserContext
{
    private ClaimsPrincipal? User => accessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;
    public string? UserId => User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    public string? UserName => User?.Identity?.Name;
    public string? Email => User?.FindFirst(ClaimTypes.Email)?.Value;
    public string? RemoteIp => accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
    public string? OwnerId => User?.FindFirst("owner_id")?.Value;
    public string? CompanyId => User?.FindFirst("company_id")?.Value;
    public string? CompanyName => User?.FindFirst("company_name")?.Value;

    private IReadOnlyList<string>? _roles;
    public IReadOnlyList<string> Roles =>
        _roles ??= User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray()
                   ?? Array.Empty<string>();

    private bool? _isAdmin;
    public bool IsAdmin => _isAdmin ??= Roles.Contains("Admin");

    private bool _companyIdChecked;
    private Guid? _companyIdParsed;
    public Guid? CompanyIdParsed
    {
        get
        {
            if (!_companyIdChecked)
            {
                _companyIdChecked = true;
                _companyIdParsed = Guid.TryParse(CompanyId, out var g) ? g : null;
            }
            return _companyIdParsed;
        }
    }

    private bool _ownerIdChecked;
    private Guid? _ownerIdParsed;
    public Guid? OwnerIdParsed
    {
        get
        {
            if (!_ownerIdChecked)
            {
                _ownerIdChecked = true;
                _ownerIdParsed = Guid.TryParse(OwnerId, out var g) ? g : null;
            }
            return _ownerIdParsed;
        }
    }
}
