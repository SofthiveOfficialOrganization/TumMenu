using Application.Auths.DTOs;
using Application.Common.Base.DTOs;
using Application.Common.Base.Page.RequestBase;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Auths.Queries;

public class GetAllUsersWithRolesQuery : PageRequest, IRequest<PaginatedListDTO<UserWithRolesDTO>>
{
    public string? Search { get; set; }
    public bool? WizardCompleted { get; set; }
}

public class GetAllUsersWithRolesHandler(
    UserManager<ApplicationUser> userManager
) : IRequestHandler<GetAllUsersWithRolesQuery, PaginatedListDTO<UserWithRolesDTO>>
{
    public async Task<PaginatedListDTO<UserWithRolesDTO>> Handle(GetAllUsersWithRolesQuery req, CancellationToken ct)
    {
        var query = userManager.Users.AsQueryable();

        if (!string.IsNullOrEmpty(req.Search))
        {
            query = query.Where(u => u.Email!.Contains(req.Search) || u.FirstName!.Contains(req.Search) || u.LastName!.Contains(req.Search));
        }

        if (req.WizardCompleted.HasValue)
        {
            query = query.Where(u => u.Owner != null && u.Owner.WizardCompleted == req.WizardCompleted.Value);
        }

        int count = await query.CountAsync(ct);
        var items = await query
            .Include(u => u.Owner)
                .ThenInclude(o => o!.Company)
            .OrderByDescending(u => u.CreatedOn)
            .Skip((req.Page - req.From) * req.PageSize)
            .Take(req.PageSize)
            .ToListAsync(ct);

        var userDtos = new List<UserWithRolesDTO>();

        foreach (var user in items)
        {
            var roles = await userManager.GetRolesAsync(user);
            userDtos.Add(new UserWithRolesDTO
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles.ToList(),
                CompanyName = user.Owner?.Company?.Title,
                CreatedOn = user.CreatedOn,
                WizardCompleted = user.Owner?.WizardCompleted
            });
        }

        return new PaginatedListDTO<UserWithRolesDTO>
        {
            Items = userDtos,
            Index = req.Page,
            Size = req.PageSize,
            From = req.From,
            Count = count,
            Pages = (int)Math.Ceiling(count / (double)req.PageSize),
            HasPrevious = req.Page > req.From,
            HasNext = req.Page < (int)Math.Ceiling(count / (double)req.PageSize) + req.From - 1
        };
    }
}
