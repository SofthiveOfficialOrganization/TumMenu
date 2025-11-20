using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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

		// -------- Identity join --------
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
		builder.Entity<Store>().HasIndex(x => x.Slug).IsUnique();
		builder.Entity<Product>().HasIndex(x => x.Slug).IsUnique();
		builder.Entity<Image>().HasIndex(i => new { i.ReferenceId, i.Type });

		// -------- ApplicationUser(1) -> Owner(1) --------
		builder.Entity<ApplicationUser>()
			.HasOne(u => u.Owner)
			.WithOne(o => o.User)
			.HasForeignKey<Owner>(o => o.ApplicationUserId)
			.OnDelete(DeleteBehavior.Cascade);

		// -------- ApplicationUser(1) -> Staff(1) --------
		builder.Entity<ApplicationUser>()
			.HasOne(u => u.Staff)
			.WithOne(s => s.User)
			.HasForeignKey<Staff>(s => s.ApplicationUserId)
			.OnDelete(DeleteBehavior.NoAction);

		// -------- Owner(1) -> Company(1) --------
		builder.Entity<Owner>()
			.HasOne(o => o.Company)
			.WithOne(c => c.Owner)
			.HasForeignKey<Company>(c => c.OwnerId)
			.IsRequired()
			.OnDelete(DeleteBehavior.Cascade);

		// -------- Company(1) -> Store(n) --------
		builder.Entity<Store>()
			.HasOne(s => s.Company)
			.WithMany(c => c.Stores)
			.HasForeignKey(s => s.CompanyId)
			.OnDelete(DeleteBehavior.Cascade);

		// -------- Store(1) -> Staff(n) --------
		builder.Entity<Staff>()
			.HasOne(s => s.Store)
			.WithMany(st => st.Staffs)
			.HasForeignKey(s => s.StoreId)
			.OnDelete(DeleteBehavior.Cascade);

		// -------- Store(1) -> Address(n) --------
		builder.Entity<Store>()
			.HasOne(s => s.Address)
			.WithOne(a => a.Store)
			.HasForeignKey<Address>(a => a.StoreId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.Entity<Menu>()
			.HasOne(m => m.Company)
			.WithOne(c => c.BaseMenu)
			.HasForeignKey<Menu>(m => m.CompanyId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.Entity<Subscription>()
			.HasOne(s => s.Plan)
			.WithMany(p => p.Subscriptions)
			.HasForeignKey(s => s.PlanId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.Entity<Invoice>()
			.HasOne(i => i.Subscription)
			.WithMany(s => s.Invoices)
			.HasForeignKey(i => i.SubscriptionId)
			.OnDelete(DeleteBehavior.Cascade);

		// -------- Product & related entities --------
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

		builder.Entity<ProductPrice>()
			.HasOne(pp => pp.Product)
			.WithMany(p => p.Prices)
			.HasForeignKey(pp => pp.ProductId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.Entity<Tag>()
			.HasOne(t => t.Product)
			.WithMany(p => p.Tags)
			.HasForeignKey(t => t.ProductId)
			.OnDelete(DeleteBehavior.Cascade);

		// -------- Plan & ExtensionPack join --------
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

		// -------- Decimal precision & Enum conversions (değişmedi) --------
		builder.Entity<Product>().Property(p => p.BasePrice).HasColumnType("decimal(18,2)");
		builder.Entity<ProductPrice>().Property(p => p.Price).HasColumnType("decimal(18,2)");
		builder.Entity<Plan>().Property(p => p.MonthlyPrice).HasColumnType("decimal(18,2)");
		builder.Entity<Plan>().Property(p => p.YearlyPrice).HasColumnType("decimal(18,2)");
		builder.Entity<Plan>().Property(p => p.WelcomeDiscountAmount).HasColumnType("decimal(18,2)");
		builder.Entity<ExtensionPack>().Property(p => p.Price).HasColumnType("decimal(18,2)");

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

