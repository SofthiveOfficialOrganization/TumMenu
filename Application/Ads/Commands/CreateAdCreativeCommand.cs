using Application.Abstractions;
using Application.Ads.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Ads.Commands;

public class CreateAdCreativeCommand : IRequest<AdCreativeDTO>, ITransactionalRequest
{
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ClickUrl { get; set; }
}

public class CreateAdCreativeCommandHandler(IRepository<AdCreative> repo, IMapper mapper) : IRequestHandler<CreateAdCreativeCommand, AdCreativeDTO>
{
    public async Task<AdCreativeDTO> Handle(CreateAdCreativeCommand request, CancellationToken ct)
    {
        var creative = mapper.Map<AdCreative>(request);
        await repo.AddAsync(creative, ct);
        return mapper.Map<AdCreativeDTO>(creative);
    }
}
