using Application.Abstractions;
using Application.Auths.DTOs;
using MediatR;

namespace Application.Auths.Commands;

public sealed record RefreshTokenCommand(string UserId, string RefreshToken) : IRequest<AuthResultDTO>;

public sealed class RefreshTokenHandler(
	IJwtTokenService tokens
) : IRequestHandler<RefreshTokenCommand, AuthResultDTO>
{
	public async Task<AuthResultDTO> Handle(RefreshTokenCommand req, CancellationToken ct)
	{
		var pair = await tokens.RefreshAsync(req.UserId, req.RefreshToken, ct);
		return new AuthResultDTO(pair.AccessToken, pair.ExpiresAt, pair.RefreshToken);
	}
}