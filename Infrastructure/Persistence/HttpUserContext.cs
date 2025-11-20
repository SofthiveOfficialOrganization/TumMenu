using Application.Abstractions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence;

public sealed class HttpUserContext(IHttpContextAccessor accessor) : IUserContext
{
	private ClaimsPrincipal? User => accessor.HttpContext?.User;

	public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;
	public string? UserId => User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
	public string? UserName => User?.Identity?.Name;
	public string? Email => User?.FindFirst(ClaimTypes.Email)?.Value;
	public IReadOnlyList<string> Roles => User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray() ?? Array.Empty<string>();
	public string? RemoteIp => accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
	public string? OwnerId => User?.FindFirst("owner_id")?.Value;
	public string? StaffId => User?.FindFirst("staff_id")?.Value;
	public string? CompanyId => User?.FindFirst("company_id")?.Value;
	public string? CompanyName => User?.FindFirst("company_name")?.Value;
	public string? StoreId => User?.FindFirst("store_id")?.Value;
	public string? StoreName => User?.FindFirst("store_name")?.Value;
}