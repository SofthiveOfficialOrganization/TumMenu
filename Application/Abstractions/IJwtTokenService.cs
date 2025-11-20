using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Abstractions;
public sealed record TokenPair(string AccessToken, DateTime ExpiresAt, string RefreshToken);

public interface IJwtTokenService
{
	Task<TokenPair> IssueAsync(ApplicationUser user, IEnumerable<string> roles, CancellationToken ct = default);
	Task<TokenPair> RefreshAsync(string userId, string refreshToken, CancellationToken ct = default);
	Task RevokeAsync(string userId, CancellationToken ct = default);
}