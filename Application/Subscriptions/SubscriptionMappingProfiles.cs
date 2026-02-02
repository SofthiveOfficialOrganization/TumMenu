using Application.Subscriptions.DTOs;
using Domain.Entities;
using Mapster;

namespace Application.Subscriptions;

public class SubscriptionMappingProfiles
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Subscription, SubscriptionLiteDTO>()
            .Map(d => d.PlanName, s => s.Plan.Title);

    }
}
