using Application.Common.Base.DTOs;

namespace Application.Subscriptions.DTOs;

public sealed class SubscriptionLiteDTO : BaseDTO
{
    public string PlanName { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
