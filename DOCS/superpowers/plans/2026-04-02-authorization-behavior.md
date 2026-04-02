# Authorization Behavior & IUserContext Performance Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add lazy-cached computed properties to `IUserContext`/`HttpUserContext` and a MediatR `AuthorizationBehavior` to eliminate per-request redundant computation and centralize authentication enforcement.

**Architecture:** `HttpUserContext` (Scoped per-request) gains `IsAdmin`, `CompanyIdParsed`, and `OwnerIdParsed` properties backed by instance-level lazy caches. A new `IAuthorizedRequest` marker interface triggers `AuthorizationBehavior` in the MediatR pipeline (between Logging and Validation) which throws `UnauthorizedAccessException` for unauthenticated requests. All handlers switch from inline `Roles.Contains("Admin")` and `Guid.TryParse(...)` to the new cached properties.

**Tech Stack:** C# 12, .NET 8, MediatR 12, ASP.NET Core, JWT Bearer authentication

---

## File Map

| Action | File |
|--------|------|
| Modify | `Application/Abstractions/IUserContext.cs` |
| Modify | `Infrastructure/Persistence/HttpUserContext.cs` |
| Create | `Application/Abstractions/IAuthorizedRequest.cs` |
| Create | `Application/Common/Behaviors/AuthorizationBehavior.cs` |
| Modify | `Application/ApplicationDI.cs` |
| Modify | `Application/Auths/Queries/GetSessionByCurrentUserQuery.cs` |
| Modify | `Application/Categories/Queries/GetCategoriesPagedByCurrentOwnerQuery.cs` |
| Modify | `Application/Categories/Queries/GetCategoryAnalysisQuery.cs` |
| Modify | `Application/Companies/Queries/GetCompanyListForSearchQuery.cs` |
| Modify | `Application/Products/Queries/GetProductAnalysisQuery.cs` |
| Modify | `Application/QRs/Queries/GetAdminCompaniesQuery.cs` |
| Modify | `Application/QRs/Queries/GetQRScanStatsQuery.cs` |
| Modify | `Application/QRs/Queries/GetQRScanStatsPerStoreQuery.cs` |
| Modify | `Application/QRs/Queries/GetStoresByCompanyQuery.cs` |
| Modify | `Application/Stores/Queries/GetStoresPagedQuery.cs` |

---

## Task 1: Extend `IUserContext` interface

**Files:**
- Modify: `Application/Abstractions/IUserContext.cs`

- [ ] **Step 1: Add the three new properties to the interface**

Replace the entire file content:

```csharp
namespace Application.Abstractions;

public interface IUserContext
{
    bool IsAuthenticated { get; }
    string? UserId { get; }
    string? UserName { get; }
    string? Email { get; }
    string? OwnerId { get; }
    string? CompanyId { get; }
    string? CompanyName { get; }
    IReadOnlyList<string> Roles { get; }
    string? RemoteIp { get; }
    bool IsAdmin { get; }
    Guid? CompanyIdParsed { get; }
    Guid? OwnerIdParsed { get; }
}
```

- [ ] **Step 2: Build to verify no compilation errors (interface change only — implementation will fail until Task 2)**

```bash
cd d:/SoftHive/Projects/2026/TumMenu
dotnet build Application/Application.csproj 2>&1 | tail -20
```

Expected: errors about `HttpUserContext` not implementing new members (that's fine — fixed in Task 2).

---

## Task 2: Update `HttpUserContext` with lazy caching

**Files:**
- Modify: `Infrastructure/Persistence/HttpUserContext.cs`

- [ ] **Step 1: Replace the entire file with the lazy-cached implementation**

```csharp
using Application.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Infrastructure.Persistence;

public sealed class HttpUserContext(IHttpContextAccessor accessor) : IUserContext
{
    private ClaimsPrincipal? User => accessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;
    public string? UserId => User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    public string? UserName => User?.Identity?.Name;
    public string? Email => User?.FindFirst(ClaimTypes.Email)?.Value;
    public string? RemoteIp => accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
    public string? OwnerId => User?.FindFirst("owner_id")?.Value;
    public string? CompanyId => User?.FindFirst("company_id")?.Value;
    public string? CompanyName => User?.FindFirst("company_name")?.Value;

    private IReadOnlyList<string>? _roles;
    public IReadOnlyList<string> Roles =>
        _roles ??= User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray()
                   ?? Array.Empty<string>();

    private bool? _isAdmin;
    public bool IsAdmin => _isAdmin ??= Roles.Contains("Admin");

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

    private bool _ownerIdChecked;
    private Guid? _ownerIdParsed;
    public Guid? OwnerIdParsed
    {
        get
        {
            if (!_ownerIdChecked)
            {
                _ownerIdChecked = true;
                _ownerIdParsed = Guid.TryParse(OwnerId, out var g) ? g : null;
            }
            return _ownerIdParsed;
        }
    }
}
```

- [ ] **Step 2: Build Infrastructure project to verify no errors**

```bash
dotnet build Infrastructure/Infrastructure.csproj 2>&1 | tail -20
```

Expected: Build succeeded.

---

## Task 3: Create `IAuthorizedRequest` marker interface

**Files:**
- Create: `Application/Abstractions/IAuthorizedRequest.cs`

- [ ] **Step 1: Create the file**

```csharp
namespace Application.Abstractions;

public interface IAuthorizedRequest { }
```

---

## Task 4: Create `AuthorizationBehavior`

**Files:**
- Create: `Application/Common/Behaviors/AuthorizationBehavior.cs`

- [ ] **Step 1: Create the behavior**

```csharp
using Application.Abstractions;
using MediatR;

namespace Application.Common.Behaviors;

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

---

## Task 5: Register `AuthorizationBehavior` in pipeline

**Files:**
- Modify: `Application/ApplicationDI.cs`

- [ ] **Step 1: Insert `AuthorizationBehavior` between Logging and Validation**

```csharp
using Application.Common.Behaviors;
using FluentValidation;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var asm = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(asm));
        services.AddValidatorsFromAssembly(asm);

        var mapsterConfig = new TypeAdapterConfig();
        mapsterConfig.Scan(asm);
        services.AddSingleton(mapsterConfig);
        services.AddScoped<IMapper, ServiceMapper>();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestLoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

        return services;
    }
}
```

- [ ] **Step 2: Build Application project**

```bash
dotnet build Application/Application.csproj 2>&1 | tail -20
```

Expected: Build succeeded.

- [ ] **Step 3: Commit Tasks 1–5**

```bash
cd d:/SoftHive/Projects/2026/TumMenu
git add Application/Abstractions/IUserContext.cs \
        Application/Abstractions/IAuthorizedRequest.cs \
        Application/Common/Behaviors/AuthorizationBehavior.cs \
        Application/ApplicationDI.cs \
        Infrastructure/Persistence/HttpUserContext.cs
git commit -m "feat: add IAuthorizedRequest, AuthorizationBehavior, and IUserContext lazy caching"
```

---

## Task 6: Update `GetSessionByCurrentUserQuery`

**Files:**
- Modify: `Application/Auths/Queries/GetSessionByCurrentUserQuery.cs`

- [ ] **Step 1: Replace inline `Guid.TryParse` calls with cached properties**

```csharp
using Application.Abstractions;
using Application.Auths.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Auths.Queries;

public sealed record GetSessionByCurrentUserQuery : IRequest<SessionDTO>, IAuthorizedRequest;

public sealed class GetSesssionByCurrentUserHandler(
    IUserContext userContext,
    IRepository<Company> repoCompany,
    IRepository<Store> repoStore
) : IRequestHandler<GetSessionByCurrentUserQuery, SessionDTO>
{
    public async Task<SessionDTO> Handle(GetSessionByCurrentUserQuery req, CancellationToken ct)
    {
        string? companyName = null;

        if (userContext.CompanyIdParsed.HasValue)
        {
            companyName = await repoCompany.Query()
                .Where(c => c.Id == userContext.CompanyIdParsed.Value)
                .Select(c => c.Title)
                .FirstOrDefaultAsync(ct);
        }

        return new SessionDTO()
        {
            UserId = userContext.UserId ?? "",
            Email = userContext.Email ?? "",
            Roles = userContext.Roles,
            OwnerId = userContext.OwnerIdParsed
        };
    }
}
```

---

## Task 7: Update `GetCategoriesPagedByCurrentOwnerQuery`

**Files:**
- Modify: `Application/Categories/Queries/GetCategoriesPagedByCurrentOwnerQuery.cs`

- [ ] **Step 1: Replace inline computations with cached properties**

```csharp
using Application.Abstractions;
using Application.Categories.DTOs;
using Application.Common.Base.DTOs;
using Application.Common.Base.Page.RequestBase;
using Domain.Entities;
using Mapster;
using MapsterMapper;
using MediatR;
using Application.Common.Base.Page;

namespace Application.Categories.Queries;

public class GetCategoriesPagedByCurrentOwnerQuery : PageRequest, IRequest<PaginatedListDTO<CategoryListDTO>>, IAuthorizedRequest
{
    public string? Search { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? StoreId { get; set; }
    public Guid? MenuId { get; set; }
}

public class GetCategoriesByCurrentOwnerHandler(
    IRepository<Category> repoCategory,
    IRepository<Company> repoCompany,
    IRepository<Store> repoStore,
    IRepository<Menu> repoMenu,
    IMapper mapper,
    IUserContext userContext
) : IRequestHandler<GetCategoriesPagedByCurrentOwnerQuery, PaginatedListDTO<CategoryListDTO>>
{
    public async Task<PaginatedListDTO<CategoryListDTO>> Handle(GetCategoriesPagedByCurrentOwnerQuery req, CancellationToken ct)
    {
        var userId = userContext.UserId;
        var isAdmin = userContext.IsAdmin;
        var contextCompanyId = userContext.CompanyIdParsed;

        var paginate = await repoCategory.GetPageListAsync(
            req,
            c =>
                (isAdmin ||
                    (contextCompanyId.HasValue
                        ? (c.Menu.CompanyId == contextCompanyId.Value || (c.Menu.StoreId != null && c.Menu.Store!.CompanyId == contextCompanyId.Value))
                        : ((c.Menu.CompanyId != null && c.Menu.Company!.Owner!.ApplicationUserId == userId) ||
                           (c.Menu.StoreId != null && c.Menu.Store!.Company.Owner!.ApplicationUserId == userId)))) &&
                (string.IsNullOrEmpty(req.Search) || c.CategoryLibraryItem.Title.Contains(req.Search)) &&
                (!req.CompanyId.HasValue || c.Menu.CompanyId == req.CompanyId.Value || (c.Menu.StoreId != null && c.Menu.Store!.CompanyId == req.CompanyId.Value)) &&
                (!req.StoreId.HasValue || c.Menu.StoreId == req.StoreId.Value) &&
                (!req.MenuId.HasValue || c.MenuId == req.MenuId.Value),
            orderBy: c => c.OrderByDescending(x => x.CreatedAt),
            enableTracking: false,
            ct: ct
        );

        var response = mapper.Map<PaginatedListDTO<CategoryListDTO>>(paginate);

        if (req.CompanyId.HasValue)
        {
            var company = await repoCompany.GetByIdAsync(req.CompanyId.Value, ct);
            if (company != null) response.FilterNames[req.CompanyId.ToString()!] = company.Title;
        }
        if (req.StoreId.HasValue)
        {
            var store = await repoStore.GetByIdAsync(req.StoreId.Value, ct);
            if (store != null) response.FilterNames[req.StoreId.ToString()!] = store.Title;
        }
        if (req.MenuId.HasValue)
        {
            var menu = await repoMenu.GetByIdAsync(req.MenuId.Value, ct);
            if (menu != null) response.FilterNames[req.MenuId.ToString()!] = menu.Title;
        }

        return response;
    }
}
```

---

## Task 8: Update `GetCategoryAnalysisQuery`

**Files:**
- Modify: `Application/Categories/Queries/GetCategoryAnalysisQuery.cs`

- [ ] **Step 1: Replace `userContext.Roles.Contains("Admin")` with `userContext.IsAdmin`**

Find this line in the `Handle` method:
```csharp
var isAdmin = userContext.Roles.Contains("Admin");
```
Replace with:
```csharp
var isAdmin = userContext.IsAdmin;
```

Also add `IAuthorizedRequest` to the record declaration:
```csharp
// Before:
public record GetCategoryAnalysisQuery(
    QRStatsGranularity Granularity = QRStatsGranularity.Monthly, 
    Guid? StoreId = null,
    Guid? CompanyId = null
) : IRequest<CategoryAnalysisResult>;

// After:
public record GetCategoryAnalysisQuery(
    QRStatsGranularity Granularity = QRStatsGranularity.Monthly, 
    Guid? StoreId = null,
    Guid? CompanyId = null
) : IRequest<CategoryAnalysisResult>, IAuthorizedRequest;
```

---

## Task 9: Update `GetCompanyListForSearchQuery`

**Files:**
- Modify: `Application/Companies/Queries/GetCompanyListForSearchQuery.cs`

- [ ] **Step 1: Replace `Roles.Contains("Admin")` with `IsAdmin`**

Find:
```csharp
var isAdmin = userContext.Roles.Contains("Admin");
```
Replace with:
```csharp
var isAdmin = userContext.IsAdmin;
```

Add `IAuthorizedRequest` to the query class declaration. Find the class/record declaration and append `, IAuthorizedRequest`.

---

## Task 10: Update `GetProductAnalysisQuery`

**Files:**
- Modify: `Application/Products/Queries/GetProductAnalysisQuery.cs`

- [ ] **Step 1: Replace `Roles.Contains("Admin")` with `IsAdmin`**

Find:
```csharp
var isAdmin = userContext.Roles.Contains("Admin");
```
Replace with:
```csharp
var isAdmin = userContext.IsAdmin;
```

Add `IAuthorizedRequest` to the query record/class declaration.

---

## Task 11: Update QR queries

**Files:**
- Modify: `Application/QRs/Queries/GetAdminCompaniesQuery.cs`
- Modify: `Application/QRs/Queries/GetQRScanStatsQuery.cs`
- Modify: `Application/QRs/Queries/GetQRScanStatsPerStoreQuery.cs`
- Modify: `Application/QRs/Queries/GetStoresByCompanyQuery.cs`

- [ ] **Step 1: Update `GetAdminCompaniesQuery` — replace `Roles.Contains` and add `IAuthorizedRequest`**

Find:
```csharp
if (!userContext.Roles.Contains("Admin"))
```
Replace with:
```csharp
if (!userContext.IsAdmin)
```

Add `IAuthorizedRequest` to the record declaration:
```csharp
// Before:
public record GetAdminCompaniesQuery : IRequest<List<AdminCompanyDTO>>;
// After:
public record GetAdminCompaniesQuery : IRequest<List<AdminCompanyDTO>>, IAuthorizedRequest;
```

- [ ] **Step 2: Update `GetQRScanStatsQuery` — replace `Roles.Contains` and add `IAuthorizedRequest`**

Find:
```csharp
var isAdmin = userContext.Roles.Contains("Admin");
```
Replace with:
```csharp
var isAdmin = userContext.IsAdmin;
```

Add `IAuthorizedRequest` to the record:
```csharp
// Before:
public record GetQRScanStatsQuery(
    QRStatsGranularity Granularity = QRStatsGranularity.Monthly, 
    Guid? StoreId = null,
    Guid? CompanyId = null
) : IRequest<List<QRStatsDTO>>;
// After:
public record GetQRScanStatsQuery(
    QRStatsGranularity Granularity = QRStatsGranularity.Monthly, 
    Guid? StoreId = null,
    Guid? CompanyId = null
) : IRequest<List<QRStatsDTO>>, IAuthorizedRequest;
```

- [ ] **Step 3: Update `GetQRScanStatsPerStoreQuery` — replace `Roles.Contains` and add `IAuthorizedRequest`**

Find:
```csharp
var isAdmin = userContext.Roles.Contains("Admin");
```
Replace with:
```csharp
var isAdmin = userContext.IsAdmin;
```

Add `IAuthorizedRequest` to the record:
```csharp
// Before:
public record GetQRScanStatsPerStoreQuery(Guid? CompanyId = null) : IRequest<List<QRStoreStatsDTO>>;
// After:
public record GetQRScanStatsPerStoreQuery(Guid? CompanyId = null) : IRequest<List<QRStoreStatsDTO>>, IAuthorizedRequest;
```

- [ ] **Step 4: Update `GetStoresByCompanyQuery` — replace `Roles.Contains` and add `IAuthorizedRequest`**

Find:
```csharp
if (!userContext.Roles.Contains("Admin"))
```
Replace with:
```csharp
if (!userContext.IsAdmin)
```

Add `IAuthorizedRequest` to the query record/class.

---

## Task 12: Update `GetStoresPagedQuery`

**Files:**
- Modify: `Application/Stores/Queries/GetStoresPagedQuery.cs`

- [ ] **Step 1: Replace inline `Roles.Contains` in expression and add `IAuthorizedRequest`**

Find in the `GetPageListAsync` expression:
```csharp
(userContext.Roles.Contains("Admin") || s.Company.Owner!.ApplicationUserId == userId),
```
Replace with:
```csharp
(userContext.IsAdmin || s.Company.Owner!.ApplicationUserId == userId),
```

Add `IAuthorizedRequest` to the query class:
```csharp
// Before:
public sealed class GetStoresPagedQuery : PageRequest, IRequest<PaginatedListDTO<StoreDTO>>
// After:
public sealed class GetStoresPagedQuery : PageRequest, IRequest<PaginatedListDTO<StoreDTO>>, IAuthorizedRequest
```

---

## Task 13: Final build and commit

- [ ] **Step 1: Full solution build**

```bash
cd d:/SoftHive/Projects/2026/TumMenu
dotnet build TumMenu.sln 2>&1 | tail -30
```

Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 2: Grep to confirm no remaining `Roles.Contains` or inline `TryParse` of userContext fields**

```bash
grep -rn "userContext\.Roles\.Contains\|Guid\.TryParse(userContext\." Application/ --include="*.cs"
```

Expected: no output.

- [ ] **Step 3: Commit all handler changes**

```bash
git add \
  Application/Auths/Queries/GetSessionByCurrentUserQuery.cs \
  Application/Categories/Queries/GetCategoriesPagedByCurrentOwnerQuery.cs \
  Application/Categories/Queries/GetCategoryAnalysisQuery.cs \
  Application/Companies/Queries/GetCompanyListForSearchQuery.cs \
  Application/Products/Queries/GetProductAnalysisQuery.cs \
  Application/QRs/Queries/GetAdminCompaniesQuery.cs \
  Application/QRs/Queries/GetQRScanStatsQuery.cs \
  Application/QRs/Queries/GetQRScanStatsPerStoreQuery.cs \
  Application/QRs/Queries/GetStoresByCompanyQuery.cs \
  Application/Stores/Queries/GetStoresPagedQuery.cs
git commit -m "refactor: use userContext.IsAdmin and CompanyIdParsed in all handlers"
```
