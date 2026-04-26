using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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
			IdentityUserToken<string>>,
		IApplicationDbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
		{
		}

		// Identity
		public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();
		public DbSet<ApplicationRole> ApplicationRoles => Set<ApplicationRole>();
		public DbSet<ApplicationUserRole> ApplicationUserRoles => Set<ApplicationUserRole>();

		// Core
		public DbSet<Owner> Owners => Set<Owner>();
		public DbSet<Staff> Staffs => Set<Staff>();
		public DbSet<Company> Companies => Set<Company>();
		public DbSet<Store> Stores => Set<Store>();
		public DbSet<Address> Addresses => Set<Address>();

		// Menu & product
		public DbSet<Menu> Menus => Set<Menu>();
		public DbSet<MenuDesign> MenuDesigns => Set<MenuDesign>();
		public DbSet<Category> Categories => Set<Category>();
		public DbSet<CategoryLibraryItem> CategoryLibraryItems => Set<CategoryLibraryItem>();
		public DbSet<CategorySuggestion> CategorySuggestions => Set<CategorySuggestion>();
		public DbSet<Product> Products => Set<Product>();
		public DbSet<ProductPrice> ProductPrices => Set<ProductPrice>();
		public DbSet<Tag> Tags => Set<Tag>();
		public DbSet<Media> Medias => Set<Media>();
		public DbSet<QRCode> QRCodes => Set<QRCode>();
		public DbSet<QRScanEvent> QRScanEvents => Set<QRScanEvent>();
		public DbSet<QRDailyStats> QRDailyStats => Set<QRDailyStats>();
		public DbSet<ProductViewEvent> ProductViewEvents => Set<ProductViewEvent>();
		public DbSet<ProductDailyStats> ProductDailyStats => Set<ProductDailyStats>();
		public DbSet<CategoryViewEvent> CategoryViewEvents => Set<CategoryViewEvent>();
		public DbSet<CategoryDailyStats> CategoryDailyStats => Set<CategoryDailyStats>();

		// Billing / subscriptions
		public DbSet<Plan> Plans => Set<Plan>();
		public DbSet<PlanFeature> PlanFeatures => Set<PlanFeature>();
		public DbSet<ExtensionPack> ExtensionPacks => Set<ExtensionPack>();
		public DbSet<ExtensionPackPlan> ExtensionPackPlans => Set<ExtensionPackPlan>();
		public DbSet<Subscription> Subscriptions => Set<Subscription>();
		public DbSet<Invoice> Invoices => Set<Invoice>();
		public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
		public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();

		// Ads
		public DbSet<AdSlot> AdSlots => Set<AdSlot>();
		public DbSet<AdCreative> AdCreatives => Set<AdCreative>();
		public DbSet<AdPlacement> AdPlacements => Set<AdPlacement>();
		public DbSet<AdImpression> AdImpressions => Set<AdImpression>();
		public DbSet<AdClick> AdClicks => Set<AdClick>();
		public DbSet<AdRevenueImport> AdRevenueImports => Set<AdRevenueImport>();

		// Blog
		public DbSet<BlogPost> BlogPosts => Set<BlogPost>();

		// Support
		public DbSet<OwnerIssueReport> OwnerIssueReports => Set<OwnerIssueReport>();

		// Misc
		public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
		public DbSet<Notification> Notifications => Set<Notification>();
		public DbSet<UsageCounter> UsageCounters => Set<UsageCounter>();
		public DbSet<WebhookEvent> WebhookEvents => Set<WebhookEvent>();
		public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

		// Many-to-many join
		public DbSet<ProductTag> ProductTags => Set<ProductTag>();


		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			ConfigureIdentity(builder);
			ConfigureCoreRelations(builder);
			ConfigureMenuAndProduct(builder);
			ConfigureBilling(builder);
			ConfigureAds(builder);
			ConfigureMisc(builder);
			ConfigurePropertyConversions(builder);
			ApplySoftDeleteQueryFilter(builder);
		}

		#region Identity

		private static void ConfigureIdentity(ModelBuilder builder)
		{
			// ApplicationUserRole (many-to-many join)
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
		}

		#endregion

		#region Core

		private static void ConfigureCoreRelations(ModelBuilder builder)
		{
			// ApplicationUser(1) -> Owner(1)
			builder.Entity<ApplicationUser>()
				.HasOne(u => u.Owner)
				.WithOne(o => o.ApplicationUser)
				.HasForeignKey<Owner>(o => o.ApplicationUserId)
				.OnDelete(DeleteBehavior.Cascade);

			// Owner(1) -> Company(1)
			builder.Entity<Owner>()
				.HasOne(o => o.Company)
				.WithOne(c => c.Owner)
				.HasForeignKey<Company>(c => c.OwnerId)
				.IsRequired(false)
				.OnDelete(DeleteBehavior.Cascade);

			// Company(1) -> Store(n)
			builder.Entity<Store>()
				.HasOne(s => s.Company)
				.WithMany(c => c.Stores)
				.HasForeignKey(s => s.CompanyId)
				.OnDelete(DeleteBehavior.Cascade);

			// Company(1) -> Media(n)
			builder.Entity<Company>()
				.HasMany(c => c.Medias)
				.WithOne()
				.HasForeignKey(m => m.CompanyId)
				.OnDelete(DeleteBehavior.NoAction);

			// Store(1) -> Staff(n)
			builder.Entity<Staff>()
				.HasOne(s => s.Store)
				.WithMany(st => st.Staffs)
				.HasForeignKey(s => s.StoreId)
				.OnDelete(DeleteBehavior.NoAction);

			// Store(1) -> Address(1)
			builder.Entity<Store>()
				.HasOne(s => s.Address)
				.WithOne(a => a.Store)
				.HasForeignKey<Address>(a => a.StoreId)
				.OnDelete(DeleteBehavior.Cascade);


			// Store(1) -> QRCode(1) - NoAction to avoid cascade cycles
			builder.Entity<Store>()
				.HasOne(s => s.QRCode)
				.WithOne(q => q.Store)
				.HasForeignKey<QRCode>(q => q.StoreId)
				.OnDelete(DeleteBehavior.NoAction);

			// Store(1) -> Media(n)
			builder.Entity<Store>()
				.HasMany(s => s.Medias)
				.WithOne()
				.HasForeignKey(m => m.StoreId)
				.OnDelete(DeleteBehavior.NoAction);


			// Company(1) -> Subscription(1)
			builder.Entity<Company>()
				.HasOne(c => c.Subscription)
				.WithOne(s => s.Company)
				.HasForeignKey<Subscription>(s => s.CompanyId)
				.OnDelete(DeleteBehavior.Cascade);

			// Company(1) -> PaymentMethod(n)
			builder.Entity<Company>()
				.HasMany(c => c.PaymentMethods)
				.WithOne()
				.HasForeignKey(pm => pm.CompanyId)
				.OnDelete(DeleteBehavior.Cascade);

			// Company(*) -> DefaultPaymentMethod (optional)
			builder.Entity<Company>()
				.HasOne<PaymentMethod>()
				.WithMany()
				.HasForeignKey(c => c.DefaultPaymentMethodId)
				.OnDelete(DeleteBehavior.NoAction);

			// Unique slugs (Active records only)
			builder.Entity<Company>()
				.HasIndex(c => c.Slug)
				.IsUnique()
				.HasFilter("[IsDeleted] = 0");

			// Ensure OwnerId is unique only for active companies
			builder.Entity<Company>()
				.HasIndex(c => c.OwnerId, "IX_Companies_OwnerId_Active")
				.IsUnique()
				.HasFilter("[IsDeleted] = 0 AND [OwnerId] IS NOT NULL");


			builder.Entity<Store>()
				.HasIndex(x => new { x.CompanyId, x.Slug })
				.IsUnique()
				.HasFilter("[IsDeleted] = 0");

			builder.Entity<Store>()
				.HasIndex(x => x.Slug)
				.IsUnique()
				.HasFilter("[IsDeleted] = 0");
		}

		#endregion

		#region Menu & Product

		private static void ConfigureMenuAndProduct(ModelBuilder builder)
		{
			// Company(1) -> Menu(n)
			builder.Entity<Menu>()
				.HasOne(m => m.Company)
				.WithMany(c => c.Menus)
				.HasForeignKey(m => m.CompanyId)
				.OnDelete(DeleteBehavior.Cascade);

			// Company(*) -> DefaultMainMenu (optional)
			builder.Entity<Company>()
				.HasOne(c => c.DefaultMainMenu)
				.WithMany()
				.HasForeignKey(c => c.DefaultMainMenuId)
				.OnDelete(DeleteBehavior.NoAction);

			// Menu(1) -> Media(n)
			builder.Entity<Menu>()
				.HasMany(m => m.Medias)
				.WithOne()
				.HasForeignKey(m => m.MenuId)
				.OnDelete(DeleteBehavior.NoAction);

			// Store(1) -> Menu(n)
			builder.Entity<Menu>()
				.HasOne(m => m.Store)
				.WithMany(s => s.Menus)
				.HasForeignKey(m => m.StoreId)
				.OnDelete(DeleteBehavior.NoAction);

			// Menu(1) -> Category(n)
			builder.Entity<Category>()
				.HasOne(ca => ca.Menu)
				.WithMany(m => m.Categories)
				.HasForeignKey(ca => ca.MenuId)
				.OnDelete(DeleteBehavior.Cascade);

			// Category Self-Referencing (Parent/Children)
			builder.Entity<Category>()
				.HasOne(c => c.Parent)
				.WithMany(c => c.SubCategories)
				.HasForeignKey(c => c.ParentId)
				.OnDelete(DeleteBehavior.NoAction);

			// CategoryLibraryItem(1) -> Category(n)
			builder.Entity<Category>()
				.HasOne(c => c.CategoryLibraryItem)
				.WithMany()
				.HasForeignKey(c => c.CategoryLibraryItemId)
				.OnDelete(DeleteBehavior.Cascade);

			// CategoryLibraryItem Self-Referencing (Parent/Children)
			builder.Entity<CategoryLibraryItem>()
				.HasOne(c => c.Parent)
				.WithMany(c => c.Children)
				.HasForeignKey(c => c.ParentId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<CategoryLibraryItem>()
				.HasIndex(c => c.Slug)
				.IsUnique()
				.HasFilter("[IsDeleted] = 0 AND [Slug] IS NOT NULL");

			// Category(1) -> Product(n)
			builder.Entity<Product>()
				.HasOne(p => p.Category)
				.WithMany(ca => ca.Products)
				.HasForeignKey(p => p.CategoryId)
				.OnDelete(DeleteBehavior.Cascade);

			// Product(1) -> Media(n)
			builder.Entity<Product>()
				.HasMany(p => p.Medias)
				.WithOne()
				.HasForeignKey(m => m.ProductId)
				.OnDelete(DeleteBehavior.NoAction);

			// CategoryLibraryItem(1) -> Media(n)
			builder.Entity<CategoryLibraryItem>()
				.HasMany(c => c.Medias)
				.WithOne(m => m.CategoryLibraryItem)
				.HasForeignKey(m => m.CategoryLibraryItemId)
				.OnDelete(DeleteBehavior.NoAction);

			// Product(1) -> ProductPrice(n)
			builder.Entity<ProductPrice>()
				.HasOne(pp => pp.Product)
				.WithMany(p => p.Prices)
				.HasForeignKey(pp => pp.ProductId)
				.OnDelete(DeleteBehavior.Cascade);


			builder.Entity<ProductTag>(b =>
			{
				b.HasKey(x => new { x.ProductId, x.TagId });

				b.HasOne(x => x.Product)
					.WithMany(p => p.ProductTags)
					.HasForeignKey(x => x.ProductId);

				b.HasOne(x => x.Tag)
					.WithMany(t => t.ProductTags)
					.HasForeignKey(x => x.TagId);
			});

			builder.Entity<ProductTag>()
				.HasQueryFilter(pt => !pt.Product.IsDeleted);

			// Media: tüm referanslar ReferenceId + Type üzerinden
			builder.Entity<Media>()
				.HasIndex(i => new { i.ReferenceId, i.Type });

			// Product slug unique (Active only)
			builder.Entity<Product>()
				.HasIndex(x => new { x.CategoryId, x.Slug })
				.IsUnique()
				.HasFilter("[IsDeleted] = 0");

			// CategorySuggestion -> Company (optional FK)
			builder.Entity<CategorySuggestion>()
				.HasOne(s => s.Company)
				.WithMany()
				.HasForeignKey(s => s.CompanyId)
				.OnDelete(DeleteBehavior.SetNull);

			// MenuDesign(1) -> Menu(n) (optional)
			builder.Entity<Menu>()
				.HasOne(m => m.MenuDesign)
				.WithMany(d => d.Menus)
				.HasForeignKey(m => m.MenuDesignId)
				.OnDelete(DeleteBehavior.SetNull);

			builder.Entity<MenuDesign>()
				.HasIndex(d => d.Slug)
				.IsUnique()
				.HasFilter("[IsDeleted] = 0");
		}

		#endregion

		#region Billing

		private static void ConfigureBilling(ModelBuilder builder)
		{
			// Plan(1) -> Subscription(n)
			builder.Entity<Subscription>()
				.HasOne(s => s.Plan)
				.WithMany(p => p.Subscriptions)
				.HasForeignKey(s => s.PlanId)
				.OnDelete(DeleteBehavior.Restrict);

			// Subscription(1) -> Invoice(n)
			builder.Entity<Invoice>()
				.HasOne(i => i.Subscription)
				.WithMany(s => s.Invoices)
				.HasForeignKey(i => i.SubscriptionId)
				.OnDelete(DeleteBehavior.Cascade);

			// Plan(1) -> PlanFeature(n)
			builder.Entity<PlanFeature>()
				.HasOne(pf => pf.Plan)
				.WithMany()
				.HasForeignKey(pf => pf.PlanId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.Entity<PlanFeature>()
				.HasIndex(pf => new { pf.PlanId, pf.Key })
				.IsUnique()
				.HasFilter("[IsDeleted] = 0");

			// Plan <-> ExtensionPack (many-to-many via ExtensionPackPlan)
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

			builder.Entity<ExtensionPackPlan>()
				.HasQueryFilter(epp => !epp.ExtensionPack.IsDeleted);
		}

		#endregion

		#region Ads

		private static void ConfigureAds(ModelBuilder builder)
		{
			// AdSlot
			builder.Entity<AdSlot>()
				.HasIndex(s => s.Key)
				.IsUnique()
				.HasFilter("[IsDeleted] = 0 AND [Key] IS NOT NULL");

			// AdPlacement -> AdSlot / AdCreative
			builder.Entity<AdPlacement>()
				.HasOne(p => p.AdSlot)
				.WithMany()
				.HasForeignKey(p => p.AdSlotId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.Entity<AdPlacement>()
				.HasOne(p => p.AdCreative)
				.WithMany()
				.HasForeignKey(p => p.AdCreativeId)
				.OnDelete(DeleteBehavior.Cascade);

			// AdClick -> AdPlacement (FK, navigation yok)
			builder.Entity<AdClick>()
				.HasOne<AdPlacement>()
				.WithMany()
				.HasForeignKey(c => c.AdPlacementId)
				.OnDelete(DeleteBehavior.Cascade);

			// AdImpression -> AdPlacement (FK, navigation yok)
			builder.Entity<AdImpression>()
				.HasOne<AdPlacement>()
				.WithMany()
				.HasForeignKey(i => i.AdPlacementId)
				.OnDelete(DeleteBehavior.Cascade);
		}

		#endregion

		#region Misc / Tracking

		private static void ConfigureMisc(ModelBuilder builder)
		{
			// QRCode / stats
			builder.Entity<QRCode>()
				.HasIndex(q => q.PublicKey)
				.IsUnique()
				.HasFilter("[IsDeleted] = 0 AND [PublicKey] IS NOT NULL");

			builder.Entity<QRCode>()
				.HasOne<Menu>()
				.WithMany()
				.HasForeignKey(q => q.MenuId)
				.OnDelete(DeleteBehavior.SetNull);

			builder.Entity<QRScanEvent>()
				.HasOne(e => e.QRCode)
				.WithMany()
				.HasForeignKey(e => e.QRCodeId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.Entity<QRDailyStats>()
				.HasOne<QRCode>()
				.WithMany()
				.HasForeignKey(s => s.QRCodeId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.Entity<QRDailyStats>()
				.HasIndex(s => new { s.QRCodeId, s.Day })
				.IsUnique()
				.HasFilter("[IsDeleted] = 0");

			// UsageCounter
			builder.Entity<UsageCounter>()
				.HasOne<Company>()
				.WithMany()
				.HasForeignKey(u => u.CompanyId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.Entity<UsageCounter>()
				.HasIndex(u => new { u.CompanyId, u.Key, u.Period })
				.IsUnique()
				.HasFilter("[IsDeleted] = 0");

			// Notification -> Company (optional)
			builder.Entity<Notification>()
				.HasOne<Company>()
				.WithMany()
				.HasForeignKey(n => n.CompanyId)
					.OnDelete(DeleteBehavior.NoAction);

			// Notification -> ToUser (optional)
			builder.Entity<Notification>()
				.HasOne<ApplicationUser>()
				.WithMany()
				.HasForeignKey(n => n.ToUserId)
				.OnDelete(DeleteBehavior.SetNull);

			// AuditLog
			builder.Entity<AuditLog>()
				.HasIndex(a => a.CreatedAt);

			builder.Entity<SystemSetting>()
				.HasIndex(s => s.Type)
				.IsUnique()
				.HasFilter("[IsDeleted] = 0");

			// Product Analytics
			builder.Entity<ProductViewEvent>(e =>
			{
				e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
				e.HasIndex(x => x.ViewedAt);
				e.HasIndex(x => x.ProductId);
			});

			builder.Entity<ProductDailyStats>(e =>
			{
				e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
				e.HasIndex(x => new { x.ProductId, x.Day }).IsUnique();
			});

			// Category Analytics
			builder.Entity<CategoryViewEvent>(e =>
			{
				e.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Cascade);
				e.HasIndex(x => x.ViewedAt);
				e.HasIndex(x => x.CategoryId);
			});

			builder.Entity<CategoryDailyStats>(e =>
			{
				e.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Cascade);
				e.HasIndex(x => new { x.CategoryId, x.Day }).IsUnique();
			});
		}

		#endregion

		#region Property configuration

		private static void ConfigurePropertyConversions(ModelBuilder builder)
		{
			// Decimal precision
			builder.Entity<Product>()
				.Property(p => p.BasePrice)
				.HasColumnType("decimal(18,2)");

			builder.Entity<ProductPrice>()
				.Property(p => p.Price)
				.HasColumnType("decimal(18,2)");

			builder.Entity<Plan>()
				.Property(p => p.MonthlyPrice)
				.HasColumnType("decimal(18,2)");

			builder.Entity<Plan>()
				.Property(p => p.YearlyPrice)
				.HasColumnType("decimal(18,2)");

			builder.Entity<Plan>()
				.Property(p => p.WelcomeDiscountAmount)
				.HasColumnType("decimal(18,2)");

			builder.Entity<ExtensionPack>()
				.Property(p => p.Price)
				.HasColumnType("decimal(18,2)");

			builder.Entity<InvoiceLine>()
				.Property(l => l.UnitPrice)
				.HasColumnType("decimal(18,2)");

			builder.Entity<InvoiceLine>()
				.Property(l => l.TaxRate)
				.HasColumnType("decimal(18,2)");

			builder.Entity<InvoiceLine>()
				.Property(l => l.LineTotal)
				.HasColumnType("decimal(18,2)");

			builder.Entity<AdRevenueImport>()
				.Property(a => a.Revenue)
				.HasColumnType("decimal(18,2)");

			// Enum conversions
			builder.Entity<Subscription>()
				.Property(s => s.BillingCycle)
				.HasConversion<int>();

			builder.Entity<Subscription>()
				.Property(s => s.Status)
				.HasConversion<int>();

			builder.Entity<CategorySuggestion>()
				.Property(s => s.Status)
				.HasConversion<int>();

			builder.Entity<Media>()
				.Property(i => i.Type)
				.HasConversion<int>();

			builder.Entity<Invoice>()
				.Property(i => i.PaymentStatus)
				.HasConversion<int>();

			builder.Entity<OwnerIssueReport>()
				.Property(r => r.Status)
				.HasConversion<int>();

			builder.Entity<SystemSetting>()
				.Property(s => s.Type)
				.HasConversion<int>();
		}

		#endregion

		#region Soft delete

		private static void ApplySoftDeleteQueryFilter(ModelBuilder builder)
		{
			// IsDeleted == false filter
			foreach(var entityType in builder.Model.GetEntityTypes()
				.Where(t => typeof(Domain.Base.BaseEntity).IsAssignableFrom(t.ClrType)))
			{
				builder.Entity(entityType.ClrType)
					.HasQueryFilter(MakeIsDeletedFilter(entityType.ClrType));
			}
		}

		private static LambdaExpression MakeIsDeletedFilter(Type entityType)
		{
			// e => !((BaseEntity)e).IsDeleted
			var parameter = Expression.Parameter(entityType, "e");
			var property = Expression.Property(parameter, nameof(Domain.Base.BaseEntity.IsDeleted));
			var body = Expression.Equal(property, Expression.Constant(false));
			var delegateType = typeof(Func<,>).MakeGenericType(entityType, typeof(bool));
			return Expression.Lambda(delegateType, body, parameter);
		}

		#endregion
	}
}
