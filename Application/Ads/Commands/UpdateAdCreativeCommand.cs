using Application.Abstractions;
using Application.Ads.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Ads.Commands;

public class UpdateAdCreativeCommand : IRequest<AdCreativeDTO>, ITransactionalRequest
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ClickUrl { get; set; }
}

public class UpdateAdCreativeCommandHandler(IRepository<AdCreative> repo, IMapper mapper) : IRequestHandler<UpdateAdCreativeCommand, AdCreativeDTO>
{
    public async Task<AdCreativeDTO> Handle(UpdateAdCreativeCommand request, CancellationToken ct)
    {
        var creative = await repo.GetByIdAsync(request.Id, ct) ?? throw new Exception("Kreatif bulunamadı");
        mapper.Map(request, creative);
        repo.Update(creative);
        return mapper.Map<AdCreativeDTO>(creative);
    }
}
