using Application.Common.Base.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Subscriptions.DTOs;

public sealed record SubscriptionLiteDTO(
	string PlanName,
	DateTime StartDate,
	DateTime? EndDate
) : BaseDTO;