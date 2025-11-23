using Application.Subscriptions.DTOs;
using Domain.Entities;
using Mapster;

namespace Application.Subscriptions
{
    public class SubscriptionMapping
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Subscription, SubscriptionLiteDTO>()
                .Map(d => d.PlanName, s => s.Plan.Name);

        }
    }
}
