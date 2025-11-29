using Application.Auths.DTOs;
using Application.Common.Base.DTOs;
using Application.Companies.DTOs;
using Application.Subscriptions.DTOs;

namespace Application.Owners.DTOs;

public sealed class OwnerDTO : BaseDTO
{
    public string? TaxNo { get; set; }
    public CompanyLiteDTO Company { get; set; } = null!;
    public SubscriptionLiteDTO? Subscription { get; set; }
    public ApplicationUserLiteDTO User { get; set; } = null!;
}


public sealed record RegisterOwnerResultDTO
{
    public string UserId { get; set; } = null!;
    public Guid OwnerId { get; set; }
    public string Email { get; set; } = null!;
}