using System.Linq;                         // LINQ (Where vb.)
using System.Linq.Expressions;
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
			base.OnModelCreating(builder);

			// -------- BaseEntity soft delete: IsDeleted == false (global filter) --------
			foreach(var et in builder.Model.GetEntityTypes()
						 .Where(t => typeof(Domain.Base.BaseEntity).IsAssignableFrom(t.ClrType)))
			{
				builder.Entity(et.ClrType)
					   .HasQueryFilter(MakeIsDeletedFilter(et.ClrType));
			}

			// -------- Identity: ApplicationUserRole join (User(1) <-> Role(1) M-N) --------
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

			// -------- Indexes --------
			builder.Entity<Company>().HasIndex(x => x.Slug).IsUnique();
			builder.Entity<Company>().HasIndex(c => c.OwnerId);
			builder.Entity<Product>().HasIndex(x => x.Slug).IsUnique();
			builder.Entity<Image>().HasIndex(i => new { i.ReferenceId, i.Type });

			// -------- ApplicationUser(1) -> Owner(1) --------
			builder.Entity<ApplicationUser>()
				.HasOne(u => u.Owner)
				.WithOne(o => o.User)
				.HasForeignKey<Owner>(o => o.ApplicationUserId)
				.OnDelete(DeleteBehavior.Cascade);

			// -------- Company(1) -> Staff(n) --------
			builder.Entity<Staff>()
				.HasOne(s => s.Company)
				.WithMany(c => c.Staff)
				.HasForeignKey(s => s.CompanyId)
				.OnDelete(DeleteBehavior.Cascade);

			// -------- ApplicationUser(1) -> Staff(1) (NoAction: cascade path riskini kırar) --------
			builder.Entity<ApplicationUser>()
				.HasOne(u => u.Staff)
				.WithOne(s => s.User)
				.HasForeignKey<Staff>(s => s.ApplicationUserId)
				.OnDelete(DeleteBehavior.NoAction); // veya Restrict()

			// -------- Owner(1) -> Company(n) --------
			builder.Entity<Company>()
				.HasOne(c => c.Owner)
				.WithMany(o => o.Companies)
				.HasForeignKey(c => c.OwnerId)
				.IsRequired()                       // opsiyonelse IsRequired(false) + SetNull
				.OnDelete(DeleteBehavior.Cascade);   // veya Restrict

			// -------- Owner(1) -> PaymentMethod(1) --------
			builder.Entity<PaymentMethod>()
				.HasOne(pm => pm.Owner)
				.WithOne(o => o.PaymentMethod)
				.HasForeignKey<PaymentMethod>(pm => pm.OwnerId)
				.OnDelete(DeleteBehavior.Cascade);

			// -------- Owner(1) -> Subscription(1) --------
			builder.Entity<Owner>()
				.HasOne(o => o.Subscription)
				.WithOne(s => s.Owner)
				.HasForeignKey<Subscription>(s => s.OwnerId)
				.IsRequired()                        // opsiyonelse IsRequired(false) + SetNull + nullable FK
				.OnDelete(DeleteBehavior.Cascade);

			// -------- Company(1) -> Address(n) --------
			builder.Entity<Address>()
				.HasOne(a => a.Company)
				.WithMany(c => c.Addresses)
				.HasForeignKey(a => a.CompanyId)
				.OnDelete(DeleteBehavior.Cascade);

			// -------- Company(1) -> Menu(n) -> Category(n) -> Product(n) --------
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

			// -------- Menu(1) -> BannerImage(0..1) (opsiyonel ref; FK Menu tarafında) --------
			builder.Entity<Menu>()
				.HasOne(m => m.BannerImage)
				.WithMany()
				.HasForeignKey(m => m.BannerImageId)
				.OnDelete(DeleteBehavior.SetNull);

			// -------- Product(1) -> ProductPrice(n) --------
			builder.Entity<ProductPrice>()
				.HasOne(pp => pp.Product)
				.WithMany(p => p.Prices)
				.HasForeignKey(pp => pp.ProductId)
				.OnDelete(DeleteBehavior.Cascade);

			// -------- Product(1) -> Tag(n) --------
			builder.Entity<Tag>()
				.HasOne(t => t.Product)
				.WithMany(p => p.Tags)
				.HasForeignKey(t => t.ProductId)
				.OnDelete(DeleteBehavior.Cascade);

			// -------- Company(1) -> QRCode(n) --------
			builder.Entity<QRCode>()
				.HasOne(q => q.Company)
				.WithMany(c => c.QRCodes)
				.HasForeignKey(q => q.CompanyId)
				.OnDelete(DeleteBehavior.Cascade);

			// -------- Plan(n) <-> ExtensionPack(n) (explicit join: ExtensionPackPlan) --------
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

			// -------- Subscription(n) -> Invoice(n) --------
			builder.Entity<Subscription>()
				.HasOne(s => s.Plan)
				.WithMany(p => p.Subscriptions)
				.HasForeignKey(s => s.PlanId)
				.OnDelete(DeleteBehavior.Restrict); // cascade path’i azaltır

			builder.Entity<Invoice>()
				.HasOne(i => i.Subscription)
				.WithMany(s => s.Invoices)
				.HasForeignKey(i => i.SubscriptionId)
				.OnDelete(DeleteBehavior.Cascade);

			// -------- DECIMAL PRECISION --------
			builder.Entity<Product>().Property(p => p.BasePrice).HasColumnType("decimal(18,2)");
			builder.Entity<ProductPrice>().Property(p => p.Price).HasColumnType("decimal(18,2)");
			builder.Entity<Plan>().Property(p => p.MonthlyPrice).HasColumnType("decimal(18,2)");
			builder.Entity<Plan>().Property(p => p.YearlyPrice).HasColumnType("decimal(18,2)");
			builder.Entity<Plan>().Property(p => p.WelcomeDiscountAmount).HasColumnType("decimal(18,2)");
			builder.Entity<ExtensionPack>().Property(p => p.Price).HasColumnType("decimal(18,2)");
			builder.Entity<Invoice>().Property(i => i.SubtotalAmount).HasColumnType("decimal(18,2)");
			builder.Entity<Invoice>().Property(i => i.TaxAmount).HasColumnType("decimal(18,2)");
			builder.Entity<Invoice>().Property(i => i.TotalAmount).HasColumnType("decimal(18,2)");

			// -------- ENUM CONVERSIONS --------
			builder.Entity<Subscription>().Property(s => s.BillingCycle).HasConversion<int>();
			builder.Entity<Subscription>().Property(s => s.Status).HasConversion<int>();
			builder.Entity<Image>().Property(i => i.Type).HasConversion<int>();
			builder.Entity<Invoice>().Property(i => i.PaymentStatus).HasConversion<int>();
		}

		// BaseEntity.IsDeleted == false filtre ifadesi
		static LambdaExpression MakeIsDeletedFilter(Type t)
		{
			var p = Expression.Parameter(t, "e");
			var prop = Expression.Property(p, nameof(Domain.Base.BaseEntity.IsDeleted));
			var body = Expression.Equal(prop, Expression.Constant(false));
			var funcType = typeof(Func<,>).MakeGenericType(t, typeof(bool));
			return Expression.Lambda(funcType, body, p);
		}
	}
}
