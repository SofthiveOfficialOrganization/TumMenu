using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
	public class ApplicationDbContext
	  : IdentityDbContext<
			ApplicationUser,
			ApplicationRole,
			string,
			IdentityUserClaim<string>,
			ApplicationUserRole,
			IdentityUserLogin<string>,
			IdentityRoleClaim<string>,
			IdentityUserToken<string>
		>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

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
		public DbSet<ExtensionPackPlan> ExtensionPackPlans => Set<ExtensionPackPlan>();

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder); // Identity tabanını kur

			// ---------- Identity: UserRole join ----------
			builder.Entity<ApplicationUserRole>(ur =>
			{
				ur.HasKey(x => new { x.UserId, x.RoleId });

				ur.HasOne(x => x.User)
				  .WithMany(u => u.UserRoles)
				  .HasForeignKey(x => x.UserId)
				  .IsRequired();

				ur.HasOne(x => x.Role)
				  .WithMany(r => r.UserRoles)
				  .HasForeignKey(x => x.RoleId)
				  .IsRequired();

				ur.ToTable("AspNetUserRoles");
			});

			// ---------- Indexes ----------
			builder.Entity<Company>().HasIndex(x => x.Slug).IsUnique();
			builder.Entity<Product>().HasIndex(x => x.Slug).IsUnique();
			builder.Entity<Image>().HasIndex(i => new { i.ReferenceId, i.Type });

			// ---------- ApplicationUser (1) -> Owner (N) ----------
			builder.Entity<Owner>()
				.HasOne(o => o.User)
				.WithMany(u => u.Owners)
				.HasForeignKey(o => o.ApplicationUserId)
				.OnDelete(DeleteBehavior.Cascade);

			// Staff -> Company  (CASCADE kalsın)
			builder.Entity<Staff>()
				.HasOne(s => s.Company)
				.WithMany(c => c.Staff)
				.HasForeignKey(s => s.CompanyId)
				.OnDelete(DeleteBehavior.Cascade);

			// Staff -> ApplicationUser  (NO ACTION / RESTRICT yap)
			builder.Entity<Staff>()
				.HasOne(s => s.User)
				.WithMany(u => u.Staffs)
				.HasForeignKey(s => s.ApplicationUserId)
				.OnDelete(DeleteBehavior.NoAction); // veya .Restrict()

			// ---------- Owner (principal) -> Company (dependent) 1–1 ----------
			builder.Entity<Company>()
				.HasOne(c => c.Owner)
				.WithOne(o => o.Company)
				.HasForeignKey<Company>(c => c.OwnerId)
				.OnDelete(DeleteBehavior.Cascade);

			// ---------- Owner (principal) -> PaymentMethod (dependent) 1–1 ----------
			builder.Entity<PaymentMethod>()
				.HasOne(pm => pm.Owner)
				.WithOne(o => o.PaymentMethod)
				.HasForeignKey<PaymentMethod>(pm => pm.OwnerId)
				.OnDelete(DeleteBehavior.Cascade);

			// ---------- Owner (principal) -> Subscription (dependent, optional) 1–1 ----------
			builder.Entity<Owner>()
				.HasOne(o => o.Subscription)
				.WithOne(s => s.Owner)
				.HasForeignKey<Subscription>(s => s.OwnerId)
				.IsRequired()
				.OnDelete(DeleteBehavior.Cascade);

			// ---------- Company -> Address (1–N) ----------
			builder.Entity<Address>()
				.HasOne(a => a.Company)
				.WithMany(c => c.Addresses)
				.HasForeignKey(a => a.CompanyId)
				.OnDelete(DeleteBehavior.Cascade);

			// ---------- Company -> Menu -> Category -> Product (cascade zinciri) ----------
			builder.Entity<Menu>()
				.HasOne(m => m.Company)
				.WithMany(c => c.Menus)
				.HasForeignKey(m => m.CompanyId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.Entity<Category>()
				.HasOne(ca => ca.Menu)
				.WithMany(m => m.Categories)
				.HasForeignKey(ca => ca.MenuId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.Entity<Product>()
				.HasOne(p => p.Category)
				.WithMany(ca => ca.Products)
				.HasForeignKey(p => p.CategoryId)
				.OnDelete(DeleteBehavior.Cascade);

			// ---------- Menu banner image (opsiyonel) ----------
			builder.Entity<Menu>()
				.HasOne(m => m.BannerImage)
				.WithMany()
				.HasForeignKey(m => m.BannerImageId)
				.OnDelete(DeleteBehavior.SetNull);

			// ---------- ProductPrice ----------
			builder.Entity<ProductPrice>()
				.HasOne(pp => pp.Product)
				.WithMany(p => p.Prices)
				.HasForeignKey(pp => pp.ProductId)
				.OnDelete(DeleteBehavior.Cascade);

			// ---------- Tag ----------
			builder.Entity<Tag>()
				.HasOne(t => t.Product)
				.WithMany(p => p.Tags)
				.HasForeignKey(t => t.ProductId)
				.OnDelete(DeleteBehavior.Cascade);

			// ---------- QRCode ----------
			builder.Entity<QRCode>()
				.HasOne(q => q.Company)
				.WithMany(c => c.QRCodes)
				.HasForeignKey(q => q.CompanyId)
				.OnDelete(DeleteBehavior.Cascade);

			// ---------- Plan <-> ExtensionPack (N–N, explicit join) ----------
			builder.Entity<ExtensionPackPlan>(eb =>
			{
				eb.ToTable("ExtensionPackPlans");
				eb.HasKey(x => new { x.ExtensionPackId, x.PlanId });

				eb.HasOne(x => x.ExtensionPack)
				  .WithMany(x => x.ExtensionPackPlans)
				  .HasForeignKey(x => x.ExtensionPackId)
				  .OnDelete(DeleteBehavior.Cascade);

				eb.HasOne(x => x.Plan)
				  .WithMany(x => x.ExtensionPacks)
				  .HasForeignKey(x => x.PlanId)
				  .OnDelete(DeleteBehavior.Cascade);
			});

			// ---------- Subscription / Invoice ----------
			builder.Entity<Subscription>()
				.HasOne(s => s.Plan)
				.WithMany(p => p.Subscriptions)
				.HasForeignKey(s => s.PlanId)
				.OnDelete(DeleteBehavior.Restrict); // cascade path’i azalt

			builder.Entity<Invoice>()
				.HasOne(i => i.Subscription)
				.WithMany(s => s.Invoices)
				.HasForeignKey(i => i.SubscriptionId)
				.OnDelete(DeleteBehavior.Cascade);

			// ---------- DECIMAL PRECISION ----------
			builder.Entity<Product>().Property(p => p.BasePrice).HasColumnType("decimal(18,2)");
			builder.Entity<ProductPrice>().Property(p => p.Price).HasColumnType("decimal(18,2)");
			builder.Entity<Plan>().Property(p => p.MonthlyPrice).HasColumnType("decimal(18,2)");
			builder.Entity<Plan>().Property(p => p.YearlyPrice).HasColumnType("decimal(18,2)");
			builder.Entity<Plan>().Property(p => p.WelcomeDiscountAmount).HasColumnType("decimal(18,2)");
			builder.Entity<ExtensionPack>().Property(p => p.Price).HasColumnType("decimal(18,2)");
			builder.Entity<Invoice>().Property(i => i.SubtotalAmount).HasColumnType("decimal(18,2)");
			builder.Entity<Invoice>().Property(i => i.TaxAmount).HasColumnType("decimal(18,2)");
			builder.Entity<Invoice>().Property(i => i.TotalAmount).HasColumnType("decimal(18,2)");

			// ---------- ENUM CONVERSIONS ----------
			builder.Entity<Subscription>().Property(s => s.BillingCycle).HasConversion<int>();
			builder.Entity<Subscription>().Property(s => s.Status).HasConversion<int>();
			builder.Entity<Image>().Property(i => i.Type).HasConversion<int>();
			builder.Entity<Invoice>().Property(i => i.PaymentStatus).HasConversion<int>();
		}
	}
}
