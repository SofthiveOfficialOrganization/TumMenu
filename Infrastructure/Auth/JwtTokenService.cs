using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Abstractions;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Auth;

public sealed class JwtOptions
{
	public string Issuer { get; set; } = "";
	public string Audience { get; set; } = "";
	public string Secret { get; set; } = "";
	public int AccessTokenMinutes { get; set; } = 15;
	public int RefreshTokenDays { get; set; } = 7;
}

public sealed class JwtTokenService : IJwtTokenService
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly JwtOptions _opt;
	private const string Provider = "TumMenu";
	private const string Name = "RefreshToken";

	public JwtTokenService(UserManager<ApplicationUser> um, IConfiguration cfg)
	{
		_userManager = um;
		_opt = cfg.GetSection("Jwt").Get<JwtOptions>() ?? throw new InvalidOperationException("Jwt section missing");
	}

	public async Task<TokenPair> IssueAsync(ApplicationUser user, IEnumerable<string> roles, string? ownerId = null, CancellationToken ct = default)
	{
		var (access, exp) = GenerateAccessToken(user, roles, ownerId);
		var refresh = GenerateRefreshToken();

		var hash = Sha256(refresh);
		await _userManager.SetAuthenticationTokenAsync(user, Provider, Name, hash);

		return new TokenPair(access, exp, refresh);
	}

	public async Task<TokenPair> RefreshAsync(string userId, string refreshToken, CancellationToken ct = default)
	{
		var user = await _userManager.FindByIdAsync(userId) ?? throw new UnauthorizedAccessException();
		var stored = await _userManager.GetAuthenticationTokenAsync(user, Provider, Name);
		if(stored is null || stored != Sha256(refreshToken))
			throw new UnauthorizedAccessException("Invalid refresh token.");

		var roles = await _userManager.GetRolesAsync(user);
		var (access, exp) = GenerateAccessToken(user, roles, ownerId: null);

		var newRefresh = GenerateRefreshToken();
		await _userManager.SetAuthenticationTokenAsync(user, Provider, Name, Sha256(newRefresh));

		return new TokenPair(access, exp, newRefresh);
	}

	public async Task RevokeAsync(string userId, CancellationToken ct = default)
	{
		var user = await _userManager.FindByIdAsync(userId);
		if(user != null)
			await _userManager.RemoveAuthenticationTokenAsync(user, Provider, Name);
	}

	private (string token, DateTime expires) GenerateAccessToken(ApplicationUser user, IEnumerable<string> roles, string? ownerId)
	{
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Secret));
		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
		var now = DateTime.UtcNow;
		var expires = now.AddMinutes(_opt.AccessTokenMinutes);

		var claims = new List<Claim>
		{
			new(JwtRegisteredClaimNames.Sub, user.Id),
			new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
			new(ClaimTypes.NameIdentifier, user.Id),
			new(ClaimTypes.Name, user.UserName ?? user.Email ?? user.Id)
		};
		if(!string.IsNullOrWhiteSpace(user.Email))
			claims.Add(new(ClaimTypes.Email, user.Email!));

		if(!string.IsNullOrEmpty(ownerId))
			claims.Add(new("owner_id", ownerId));

		claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

		var jwt = new JwtSecurityToken(
			issuer: _opt.Issuer,
			audience: _opt.Audience,
			claims: claims,
			notBefore: now,
			expires: expires,
			signingCredentials: creds);

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
}

