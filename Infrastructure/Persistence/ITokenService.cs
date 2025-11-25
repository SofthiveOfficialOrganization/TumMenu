using System.Security.Claims;

namespace Infrastructure.Persistence;

public interface ITokenService
{
    string CreateToken(IEnumerable<Claim> claims, DateTime expires);
    ClaimsPrincipal? ReadToken(string jwt);
}
