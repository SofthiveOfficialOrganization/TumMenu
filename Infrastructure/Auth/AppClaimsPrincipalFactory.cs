using System.Security.Claims;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Auth;

public sealed class AppClaimsPrincipalFactory
	: UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
{
	private readonly ApplicationDbContext _db;

	public AppClaimsPrincipalFactory(
		UserManager<ApplicationUser> userManager,
		RoleManager<ApplicationRole> roleManager,
		IOptions<IdentityOptions> optionsAccessor,
		ApplicationDbContext db)
		: base(userManager, roleManager, optionsAccessor)
	{
		_db = db;
	}

	protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
	{
		var identity = await base.GenerateClaimsAsync(user);

		var owner = await _db.Owners
			.Include(o => o.Company)
			.FirstOrDefaultAsync(o => o.ApplicationUserId == user.Id);

		if(owner is not null)
		{
			identity.AddClaim(new Claim("owner_id", owner.Id.ToString()));

			if(owner.Company is not null)
			{
				identity.AddClaim(new Claim("company_id", owner.Company.Id.ToString()));
				identity.AddClaim(new Claim("company_name", owner.Company.Name));
			}
		}

		return identity;
	}
}
