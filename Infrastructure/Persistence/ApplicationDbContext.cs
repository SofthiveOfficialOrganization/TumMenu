using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
		{
		}

		public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();
		public DbSet<Staff> Staffs => Set<Staff>();
		public DbSet<Owner> Owners => Set<Owner>();
		public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
		public DbSet<Company> Companies => Set<Company>();
		public DbSet<Menu> Menus => Set<Menu>();
		public DbSet<Category> Categories => Set<Category>();
		public DbSet<Product> Products => Set<Product>();
		public DbSet<ProductPrice> ProductPrices => Set<ProductPrice>();
		public DbSet<Tag> Tags => Set<Tag>();
		public DbSet<Image> Images => Set<Image>();
		public DbSet<QRCode> QRCodes => Set<QRCode>();
		public DbSet<Address> Addresses => Set<Address>();
		public DbSet<Plan> Plans => Set<Plan>();
		public DbSet<ExtensionPack> ExtensionPacks => Set<ExtensionPack>();
		public DbSet<Subscription> Subscriptions => Set<Subscription>();
		public DbSet<Invoice> Invoices => Set<Invoice>();

	}
}
