using Application.Abstractions;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.CustomerOrderRequests.Commands;

public sealed record MarkOrderRequestSeenCommand(Guid Id) : IRequest, IAuthorizedRequest;
public sealed record CompleteOrderRequestCommand(Guid Id) : IRequest, IAuthorizedRequest;

public sealed class MarkOrderRequestSeenCommandHandler(IApplicationDbContext db, IUserContext userContext)
    : IRequestHandler<MarkOrderRequestSeenCommand>
{
    public async Task Handle(MarkOrderRequestSeenCommand request, CancellationToken ct)
    {
        await OrderRequestStatusUpdater.UpdateAsync(
            db,
            userContext,
            request.Id,
            CustomerOrderRequestStatus.Seen,
            ct);
    }
}

public sealed class CompleteOrderRequestCommandHandler(IApplicationDbContext db, IUserContext userContext)
    : IRequestHandler<CompleteOrderRequestCommand>
{
    public async Task Handle(CompleteOrderRequestCommand request, CancellationToken ct)
    {
        await OrderRequestStatusUpdater.UpdateAsync(
            db,
            userContext,
            request.Id,
            CustomerOrderRequestStatus.Completed,
            ct);
    }
}

internal static class OrderRequestStatusUpdater
{
    public static async Task UpdateAsync(
        IApplicationDbContext db,
        IUserContext userContext,
        Guid orderId,
        CustomerOrderRequestStatus nextStatus,
        CancellationToken ct)
    {
        var order = await db.CustomerOrderRequests.FirstOrDefaultAsync(x => x.Id == orderId, ct);
        if (order is null)
        {
            throw new NotFoundAppException("Sipariş talebi bulunamadı.");
        }

        if (!userContext.IsAdmin)
        {
            var companyId = await db.Companies
                .Where(x => x.Owner != null && x.Owner.ApplicationUserId == userContext.UserId)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(ct);

            if (companyId != order.CompanyId)
            {
                throw new ForbiddenAppException("Bu sipariş talebini güncelleyemezsiniz.");
            }
        }

        order.Status = nextStatus;
        order.Modified(userContext.UserId);
        db.CustomerOrderRequests.Update(order);
        await db.SaveChangesAsync(ct);
    }
}
