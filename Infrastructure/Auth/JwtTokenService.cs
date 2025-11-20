using Application.Abstractions;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Auth;

public sealed class JwtOptions
{
	public string Issuer { get; init; } = default!;
	public string Audience { get; init; } = default!;
	public string Secret { get; init; } = default!;
	public int AccessTokenMinutes { get; init; } = 60;
	public int RefreshTokenDays { get; init; } = 7;

	public string RefreshTokenProvider { get; init; } = default!;
	public string RefreshTokenName { get; init; } = default!;
}

public static class CustomClaimTypes
{
	public const string CompanyId = "company_id";
	public const string CompanyName = "company_name";
	public const string OwnerId = "owner_id";
	public const string StaffId = "staff_id";
	public const string StoreId = "store_id";
	public const string StoreName = "store_name";
}

public sealed class JwtTokenService(
	UserManager<ApplicationUser> userManager,
	IConfiguration configuration,
	ApplicationDbContext applicationDbContext
	) : IJwtTokenService
{
	private readonly JwtOptions jwtOptions = configuration.GetSection("Jwt").Get<JwtOptions>() ?? throw new InvalidOperationException("Jwt section missing");

	public async Task<TokenPair> IssueAsync(ApplicationUser user, IEnumerable<string> roles, CancellationToken ct = default)
	{
		var rel = await GetRelationsAsync(user.Id, ct);

		var (access, exp) = GenerateAccessToken(user, roles, rel);
		var refresh = GenerateRefreshToken();

		var hash = Sha256(refresh);
		await userManager.SetAuthenticationTokenAsync(
			user,
			jwtOptions.RefreshTokenProvider,
			jwtOptions.RefreshTokenName,
			hash
		);

		return new TokenPair(access, exp, refresh);
	}

	public async Task<TokenPair> RefreshAsync(string userId, string refreshToken, CancellationToken ct = default)
	{
		var user = await userManager.FindByIdAsync(userId) ?? throw new UnauthorizedAccessException();
		var stored = await userManager.GetAuthenticationTokenAsync(user, jwtOptions.RefreshTokenProvider, jwtOptions.RefreshTokenName);

		if(stored is null || stored != Sha256(refreshToken))
			throw new UnauthorizedAccessException("Invalid refresh token.");

		var roles = await userManager.GetRolesAsync(user);

		var rel = await GetRelationsAsync(user.Id, ct);
		var (access, exp) = GenerateAccessToken(user, roles, rel);

		var newRefresh = GenerateRefreshToken();
		await userManager.SetAuthenticationTokenAsync(
			user,
			jwtOptions.RefreshTokenProvider,
			jwtOptions.RefreshTokenName,
			Sha256(newRefresh)
		);

		return new TokenPair(access, exp, newRefresh);
	}

	public async Task RevokeAsync(string userId, CancellationToken ct = default)
	{
		var user = await userManager.FindByIdAsync(userId);
		if(user != null)
		{
			await userManager.RemoveAuthenticationTokenAsync(
				user, jwtOptions.RefreshTokenProvider, jwtOptions.RefreshTokenName
			);
		}
	}

	private async Task<SystemRelations> GetRelationsAsync(string userId, CancellationToken ct)
	{

		var owner = await applicationDbContext.Set<Owner>()
			.Where(owner => owner.ApplicationUserId == userId)
				.Include(o => o.Company).FirstOrDefaultAsync(ct);

		var staff = await applicationDbContext.Set<Staff>()
			.Where(staff => staff.ApplicationUserId == userId)
				.Include(s => s.Store).FirstOrDefaultAsync(ct);
		SystemRelations relations = new();
		if(owner != null)
			relations.OwnerId = owner.Id;
		if(owner != null && owner!.Company != null)
			relations.CompanyId = owner.Company.Id;
		if(staff != null)
			relations.StaffId = staff.Id;
		if(staff != null && staff!.Store != null)
			relations.StoreId = staff.Store.Id;
		return relations;
	}

	private (string token, DateTime expires) GenerateAccessToken(
		ApplicationUser user,
		IEnumerable<string> roles,
		SystemRelations rel
	)
	{
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret));
		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
		var now = DateTime.UtcNow;
		var expires = now.AddMinutes(jwtOptions.AccessTokenMinutes);

		var claims = new List<Claim>
		{
			new(JwtRegisteredClaimNames.Sub, user.Id),
			new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
			new(ClaimTypes.NameIdentifier, user.Id),
			new(ClaimTypes.Name, user.UserName ?? user.Email ?? user.Id)
		};

		if(!string.IsNullOrWhiteSpace(user.Email))
			claims.Add(new(ClaimTypes.Email, user.Email!));

		// Rol claimleri
		claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

		// İlişki claimleri
		if(rel.CompanyId.HasValue)
			claims.Add(new(CustomClaimTypes.CompanyId, rel.CompanyId.Value.ToString()));
		if(rel.OwnerId.HasValue)
			claims.Add(new(CustomClaimTypes.OwnerId, rel.OwnerId.Value.ToString()));
		if(rel.StaffId.HasValue)
			claims.Add(new(CustomClaimTypes.StaffId, rel.StaffId.Value.ToString()));

		var jwt = new JwtSecurityToken(
			issuer: jwtOptions.Issuer,
			audience: jwtOptions.Audience,
			claims: claims,
			notBefore: now,
			expires: expires,
			signingCredentials: creds
		);

		var token = new JwtSecurityTokenHandler().WriteToken(jwt);
		return (token, expires);
	}

	private static string GenerateRefreshToken()
	{
		var bytes = RandomNumberGenerator.GetBytes(64);
		return Convert.ToBase64String(bytes);
	}

	private static string Sha256(string input)
	{
		using var sha = SHA256.Create();
		var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
		return Convert.ToBase64String(bytes);
	}
	private class SystemRelations
	{
		public Guid? CompanyId { get; set; }
		public Guid? OwnerId { get; set; }
		public Guid? StaffId { get; set; }
		public Guid? StoreId { get; set; }
	}
}
