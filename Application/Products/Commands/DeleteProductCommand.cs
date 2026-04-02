using Application.Abstractions;
using Application.Common.Helpers;
using Domain.Entities;
using Mapster;
using MapsterMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Products.Commands;

public class DeleteProductCommand : IRequest<Unit>, ITransactionalRequest, IEntityAuditableCommand
{
	public Guid ProductId { get; set; }
	public string ActionName => "Ürün silindi";
	public Guid EntityId => ProductId;
}

public class DeleteProductCommandHandler(
	IRepository<Product> repoProduct
) : IRequestHandler<DeleteProductCommand, Unit>
{
	public async Task<Unit> Handle(DeleteProductCommand req, CancellationToken ct)
	{
		var product = await repoProduct.GetByIdAsync(req.ProductId, ct).EnsureFound("Silinmeye çalışılan ürün bulunamadı");
		repoProduct.SoftDelete(product!);
		return Unit.Value;
	}
}
