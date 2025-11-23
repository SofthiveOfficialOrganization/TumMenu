using Application.Auths.DTOs;
using Application.Common.Base.DTOs;
using Application.Companies.DTOs;
using Application.Subscriptions.DTOs;

namespace Application.Owners.DTOs;

public sealed record OwnerDTO(
    string? TaxNo,
    CompanyLiteDTO Company,
    SubscriptionLiteDTO? Subscription,
    ApplicationUserLiteDTO User
) : BaseDTO;