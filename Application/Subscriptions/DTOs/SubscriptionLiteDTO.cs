using Application.Common.Base.DTOs;

namespace Application.Subscriptions.DTOs;

public sealed record SubscriptionLiteDTO(
    string PlanName,
    DateTime StartDate,
    DateTime? EndDate
) : BaseDTO;