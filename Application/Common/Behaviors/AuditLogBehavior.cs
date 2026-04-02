using Application.Abstractions;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Common.Behaviors;

public sealed class AuditLogBehavior<TRequest, TResponse>(
    IApplicationDbContext db,
    IUserContext userContext)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (request is not IAuditableCommand auditable)
            return await next();

        var response = await next();

        var entityName = ExtractEntityName(typeof(TRequest).Name);
        var entityId = request is IEntityAuditableCommand entityAuditable
            ? entityAuditable.EntityId
            : Guid.Empty;

        var log = new AuditLog
        {
            UserId = userContext.UserId ?? "unknown",
            Action = auditable.ActionName,
            Entity = entityName,
            EntityId = entityId,
            ChangesJson = "{}",
            Ip = userContext.RemoteIp
        };
        log.Created(userContext.UserId);

        await db.AuditLogs.AddAsync(log, ct);
        await db.SaveChangesAsync(ct);

        return response;
    }

    /// <summary>
    /// "CreateCompanyCommand" → "Company"
    /// "DeleteUserCommand"    → "User"
    /// </summary>
    private static string ExtractEntityName(string commandName)
    {
        var withoutCommand = commandName.EndsWith("Command")
            ? commandName[..^"Command".Length]
            : commandName;

        foreach (var prefix in new[] { "Create", "Update", "Delete", "Approve", "Submit", "Mark" })
        {
            if (withoutCommand.StartsWith(prefix))
                return withoutCommand[prefix.Length..];
        }

        return withoutCommand;
    }
}
