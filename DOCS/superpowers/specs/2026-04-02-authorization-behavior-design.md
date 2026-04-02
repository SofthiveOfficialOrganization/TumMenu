# Authorization Behavior & IUserContext Performance Design

**Date:** 2026-04-02  
**Status:** Approved

## Problem

Every handler that performs admin/owner checks re-computes the same values on each request:

1. `userContext.Roles.Contains("Admin")` — triggers a new LINQ enumerate + `ToArray()` on the JWT claims collection every call
2. `Guid.TryParse(userContext.CompanyId, out var companyId)` — repeated string-to-Guid parse in multiple handlers
3. No centralized authentication gate — any `[Authorize]`-decorated endpoint that forgets the inline check silently passes

## Goals

- Eliminate redundant computation per request (O(1) cache instead of repeated LINQ)
- Centralize authentication-only (401) enforcement in the MediatR pipeline
- Minimal change to existing handler logic and project conventions
- Follow existing `ITransactionalRequest` / `TransactionBehavior` pattern exactly

## Non-Goals

- Moving authorization *logic* (company scope, owner scope) out of handlers — this stays in handlers
- Distributed caching or cross-request caching of user data
- Policy-based authorization or attribute-driven permission declarations

---

## Design

### 1. `IUserContext` — New Computed Properties

Add three new properties to the interface:

```csharp
bool IsAdmin { get; }
Guid? CompanyIdParsed { get; }
Guid? OwnerIdParsed { get; }
```

**Rules:**
- `IsAdmin` returns `true` if `Roles` contains `"Admin"`
- `CompanyIdParsed` returns the parsed `Guid` of `CompanyId`, or `null` if absent/invalid
- `OwnerIdParsed` returns the parsed `Guid` of `OwnerId`, or `null` if absent/invalid

### 2. `HttpUserContext` — Lazy Caching

`HttpUserContext` is registered as **Scoped** (per-request), so instance fields are safe to use as a per-request cache.

**`Roles`** — cache the result of `FindAll + ToArray` on first access:
```csharp
private IReadOnlyList<string>? _roles;
public IReadOnlyList<string> Roles =>
    _roles ??= User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray()
               ?? Array.Empty<string>();
```

**`IsAdmin`** — single boolean cached after first `Roles` access:
```csharp
private bool? _isAdmin;
public bool IsAdmin => _isAdmin ??= Roles.Contains("Admin");
```

**`CompanyIdParsed`** — parse once, nullable result:
```csharp
private bool _companyIdChecked;
private Guid? _companyIdParsed;
public Guid? CompanyIdParsed
{
    get
    {
        if (!_companyIdChecked)
        {
            _companyIdChecked = true;
            _companyIdParsed = Guid.TryParse(CompanyId, out var g) ? g : null;
        }
        return _companyIdParsed;
    }
}
```

Same pattern for `OwnerIdParsed`.

### 3. `IAuthorizedRequest` Marker Interface

```csharp
// Application/Abstractions/IAuthorizedRequest.cs
namespace Application.Abstractions;

public interface IAuthorizedRequest { }
```

Any command or query that requires an authenticated user implements this interface. Scope: authentication only (is the user logged in?). Authorization logic (what can they see?) remains in the handler.

### 4. `AuthorizationBehavior`

```csharp
// Application/Common/Behaviors/AuthorizationBehavior.cs
public sealed class AuthorizationBehavior<TRequest, TResponse>(IUserContext userContext)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (request is IAuthorizedRequest && !userContext.IsAuthenticated)
            throw new UnauthorizedAccessException();

        return await next();
    }
}
```

### 5. Pipeline Registration Order

```csharp
// ApplicationDI.cs — add AuthorizationBehavior between Logging and Validation
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestLoggingBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
```

Order rationale: log everything → check auth early (fail fast before validation) → validate → transact.

### 6. Handler Updates

All handlers that currently do:
```csharp
var isAdmin = userContext.Roles.Contains("Admin");
var hasContextCompanyId = Guid.TryParse(userContext.CompanyId, out var contextCompanyId);
```

Switch to:
```csharp
var isAdmin = userContext.IsAdmin;
var contextCompanyId = userContext.CompanyIdParsed; // nullable Guid
```

The query expression predicates are unchanged in structure — only the local variables feeding them change source.

Handlers that require authentication add `IAuthorizedRequest` to their request class:
```csharp
public class GetProductsPagedByCurrentOwnerQuery
    : PageRequest, IRequest<PaginatedListDTO<ProductListDTO>>, IAuthorizedRequest
```

---

## Affected Files

| File | Change |
|------|--------|
| `Application/Abstractions/IUserContext.cs` | Add `IsAdmin`, `CompanyIdParsed`, `OwnerIdParsed` |
| `Infrastructure/Persistence/HttpUserContext.cs` | Lazy-cache `Roles`, implement new properties |
| `Application/Abstractions/IAuthorizedRequest.cs` | New marker interface |
| `Application/Common/Behaviors/AuthorizationBehavior.cs` | New behavior |
| `Application/ApplicationDI.cs` | Register `AuthorizationBehavior` |
| All handlers using `userContext.Roles.Contains("Admin")` | Switch to `userContext.IsAdmin` / `CompanyIdParsed` |

## Testing

- `AuthorizationBehavior` unit test: unauthenticated + `IAuthorizedRequest` → throws `UnauthorizedAccessException`
- `AuthorizationBehavior` unit test: unauthenticated + non-`IAuthorizedRequest` → passes through
- `HttpUserContext` unit test: `Roles` property called multiple times → `FindAll` called only once
- `HttpUserContext` unit test: `IsAdmin` with and without Admin role
