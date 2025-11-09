using Application.Subscriptions.DTOs;
using Domain.Entities;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
