using Domain.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Domain.Entities
{
	public class Company : BaseEntity
	{
		[MaxLength(200)] public string Name { get; set; } = null!;
		[MaxLength(30)] public string Slug { get; set; } = null!;

		public Guid OwnerId { get; set; }
		public Owner Owner { get; set; } = null!;
		public Menu? BaseMenu { get; set; }

		public Subscription? Subscription { get; set; }

		public Guid? DefaultPaymentMethodId { get; set; }
		public ICollection<PaymentMethod> PaymentMethods { get; set; } = [];

		public ICollection<Store> Stores { get; set; } = [];
		public ICollection<Image> Images { get; set; } = [];
	}
}
