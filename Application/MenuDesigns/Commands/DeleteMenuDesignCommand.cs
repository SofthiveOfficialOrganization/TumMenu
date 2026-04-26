using Application.Abstractions;
using Domain.Entities;
using MediatR;

namespace Application.MenuDesigns.Commands;

public sealed class DeleteMenuDesignCommand : IRequest<Unit>, ITransactionalRequest, IEntityAuditableCommand
{
    public Guid Id { get; set; }
    public string ActionName => "Menü tasarımı silindi";
    public Guid EntityId => Id;
}

public class DeleteMenuDesignHandler(
    IRepository<MenuDesign> repoDesign
) : IRequestHandler<DeleteMenuDesignCommand, Unit>
{
    public async Task<Unit> Handle(DeleteMenuDesignCommand req, CancellationToken ct)
    {
        var design = await repoDesign.GetByIdAsync(req.Id, ct)
            ?? throw new KeyNotFoundException("Tasarım bulunamadı.");

        repoDesign.SoftDelete(design);
        return Unit.Value;
    }
}
