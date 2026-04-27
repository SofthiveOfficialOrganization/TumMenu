using Application.Abstractions;
using Application.MenuDesigns.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.MenuDesigns.Queries;

public sealed record GetMenuDesignByIdQuery(Guid Id) : IRequest<MenuDesignDTO>;

public class GetMenuDesignByIdHandler(
    IRepository<MenuDesign> repoDesign
) : IRequestHandler<GetMenuDesignByIdQuery, MenuDesignDTO>
{
    public async Task<MenuDesignDTO> Handle(GetMenuDesignByIdQuery req, CancellationToken ct)
    {
        var d = await repoDesign.Query()
            .FirstOrDefaultAsync(x => x.Id == req.Id, ct)
            ?? throw new KeyNotFoundException("Tasarım bulunamadı.");

        return d.ToDto();
    }
}
