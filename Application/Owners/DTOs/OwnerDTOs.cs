using Application.Auths.DTOs;
using Application.Common.Base.DTOs;
using Application.Companies.DTOs;
using Application.Subscriptions.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Owners.DTOs;
public sealed record OwnerDTO(
	string? TaxNo,
	CompanyLiteDTO Company,
	SubscriptionLiteDTO? Subscription,
	ApplicationUserLiteDTO User
) : BaseDTO;