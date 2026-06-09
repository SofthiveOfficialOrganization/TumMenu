using System.Net;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebUI.IntegrationTests.Infrastructure;

namespace WebUI.IntegrationTests.Integration.Product;

public class ProductEditTests : IClassFixture<TumMenuWebAppFactory>
{
	private readonly TumMenuWebAppFactory _factory;

	public ProductEditTests(TumMenuWebAppFactory factory)
	{
		_factory = factory;
		TestDbSeeder.SeedAsync(factory.Services).GetAwaiter().GetResult();
	}

	[Fact]
	public async Task PostEdit_WithPriceOption_RedirectsOnSuccess()
	{
		var (productId, categoryId) = await SeedProductAsync();
		var client = await AuthHelper.GetAuthenticatedClientAsync(_factory, "Owner");
		var editUrl = $"/Admin/Product/Edit?id={productId}";
		var token = await AntiforgeryHelper.GetTokenAsync(client, editUrl);

		var response = await client.PostAsync("/Admin/Product/Edit", CreateFormContent(
			("Id", productId.ToString()),
			("CategoryId", categoryId.ToString()),
			("Title", "Karışık Ayvalık Tost"),
			("Description", "Sucuk, Kaşar"),
			("BasePrice", "170"),
			("SortOrder", "1"),
			("Prices[0].Size", "Menü (Patates ve İçecek)"),
			("Prices[0].Price", "320"),
			("IsActive", "true"),
			("IsVegan", "false"),
			("IsVegetarian", "false"),
			("__RequestVerificationToken", token)));

		Assert.True(
			response.StatusCode is HttpStatusCode.Redirect or HttpStatusCode.Found,
			$"Expected redirect but got {response.StatusCode}. Body: {await response.Content.ReadAsStringAsync()}");

		using var scope = _factory.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		var saved = await db.Products
			.AsNoTracking()
			.Include(p => p.Prices)
			.FirstAsync(p => p.Id == productId);

		Assert.Equal("Karışık Ayvalık Tost", saved.Title);
		Assert.Equal("Sucuk, Kaşar", saved.Description);
		Assert.Equal(170m, saved.BasePrice);
		Assert.Single(saved.Prices);
		Assert.Equal("Menü (Patates ve İçecek)", saved.Prices.Single().Size);
		Assert.Equal(320m, saved.Prices.Single().Price);
	}

	[Fact]
	public async Task GetEdit_RendersDecimalPriceWithHtmlNumberCompatibleValue()
	{
		var (productId, _) = await SeedProductAsync(basePrice: 100.50m);
		var client = await AuthHelper.GetAuthenticatedClientAsync(_factory, "Owner");

		var response = await client.GetAsync($"/Admin/Product/Edit?id={productId}");

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		var html = await response.Content.ReadAsStringAsync();
		Assert.Contains("name=\"BasePrice\"", html);
		Assert.Contains("value=\"100.50\"", html);
		Assert.DoesNotContain("value=\"100,50\"", html);
	}

	[Fact]
	public async Task PostEdit_WithBrowserInvariantFields_UpdatesProduct()
	{
		var (productId, categoryId) = await SeedProductAsync();
		var client = await AuthHelper.GetAuthenticatedClientAsync(_factory, "Owner");
		var editUrl = $"/Admin/Product/Edit?id={productId}";
		var token = await AntiforgeryHelper.GetTokenAsync(client, editUrl);
		var returnUrl = $"/Admin/Category/Details/{categoryId}?returnUrl=%2FAdmin%2FMenu";

		var response = await client.PostAsync("/Admin/Product/Edit", CreateFormContent(
			("Id", productId.ToString()),
			("CategoryId", categoryId.ToString()),
			("returnUrl", returnUrl),
			("Title", "Kıymalı Pide "),
			("Description", ""),
			("BasePrice", "240.00"),
			("__Invariant", "BasePrice"),
			("EstimatedPreparationTimeInMinutes", "60"),
			("__Invariant", "EstimatedPreparationTimeInMinutes"),
			("SortOrder", "1"),
			("__Invariant", "SortOrder"),
			("Prices[0].Size", "1.5 Porsiyon"),
			("Prices[0].Price", "330.00"),
			("Allergens", ""),
			("IsActive", "true"),
			("IsActive", "false"),
			("IsVegan", "false"),
			("IsVegetarian", "false"),
			("__RequestVerificationToken", token)));

		Assert.True(
			response.StatusCode is HttpStatusCode.Redirect or HttpStatusCode.Found,
			$"Expected redirect but got {response.StatusCode}. Body: {await response.Content.ReadAsStringAsync()}");

		using var scope = _factory.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		var saved = await db.Products
			.AsNoTracking()
			.Include(p => p.Prices)
			.FirstAsync(p => p.Id == productId);

		Assert.Equal("Kıymalı Pide ", saved.Title);
		Assert.Equal(240m, saved.BasePrice);
		Assert.Equal(60, saved.EstimatedPreparationTimeInMinutes);
		Assert.True(saved.IsActive);
		Assert.False(saved.IsVegan);
		Assert.False(saved.IsVegetarian);
		Assert.Single(saved.Prices);
		Assert.Equal("1.5 Porsiyon", saved.Prices.Single().Size);
		Assert.Equal(330m, saved.Prices.Single().Price);
	}

	[Fact]
	public async Task PostEdit_WithInvalidBasePrice_ReturnsEditFormNotServerError()
	{
		var (productId, categoryId) = await SeedProductAsync();
		var client = await AuthHelper.GetAuthenticatedClientAsync(_factory, "Owner");
		var editUrl = $"/Admin/Product/Edit?id={productId}";
		var token = await AntiforgeryHelper.GetTokenAsync(client, editUrl);

		var response = await client.PostAsync("/Admin/Product/Edit", CreateFormContent(
			("Id", productId.ToString()),
			("CategoryId", categoryId.ToString()),
			("Title", "Karışık Ayvalık Tost"),
			("BasePrice", "99999"),
			("SortOrder", "1"),
			("IsActive", "true"),
			("IsVegan", "false"),
			("IsVegetarian", "false"),
			("__RequestVerificationToken", token)));

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		var html = await response.Content.ReadAsStringAsync();
		Assert.Contains("horizon-admin", html);
		Assert.Contains("Değişiklikleri Kaydet", html);
		Assert.DoesNotContain("Ana sayfaya dön", html);
	}

	[Fact]
	public async Task PostUpdate_LegacyActionName_StillAccepted()
	{
		var (productId, categoryId) = await SeedProductAsync();
		var client = await AuthHelper.GetAuthenticatedClientAsync(_factory, "Owner");
		var editUrl = $"/Admin/Product/Edit?id={productId}";
		var token = await AntiforgeryHelper.GetTokenAsync(client, editUrl);

		var response = await client.PostAsync("/Admin/Product/Update", CreateFormContent(
			("Id", productId.ToString()),
			("CategoryId", categoryId.ToString()),
			("Title", "Legacy Update Route"),
			("BasePrice", "150"),
			("SortOrder", "1"),
			("IsActive", "true"),
			("IsVegan", "false"),
			("IsVegetarian", "false"),
			("__RequestVerificationToken", token)));

		Assert.True(
			response.StatusCode is HttpStatusCode.Redirect or HttpStatusCode.Found,
			$"Expected redirect but got {response.StatusCode}");
	}

	private static FormUrlEncodedContent CreateFormContent(params (string Key, string Value)[] fields)
	{
		var pairs = fields.Select(f => new KeyValuePair<string, string>(f.Key, f.Value));
		return new FormUrlEncodedContent(pairs);
	}

	private async Task<(Guid ProductId, Guid CategoryId)> SeedProductAsync(decimal basePrice = 100m)
	{
		using var scope = _factory.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		var suffix = Guid.NewGuid().ToString("N")[..8];
		var libItem = new CategoryLibraryItem { Title = "Tostlar", Slug = $"tostlar-edit-{suffix}" };
		db.CategoryLibraryItems.Add(libItem);

		var category = new Category
		{
			MenuId = TestDbSeeder.MenuId,
			CategoryLibraryItemId = libItem.Id,
			SortOrder = 1,
			IsActive = true
		};
		db.Categories.Add(category);

		var product = new Domain.Entities.Product
		{
			Title = "Test Tost",
			Slug = $"test-tost-edit-{suffix}",
			BasePrice = basePrice,
			CategoryId = category.Id,
			SortOrder = 1,
			IsActive = true
		};
		db.Products.Add(product);
		await db.SaveChangesAsync();

		return (product.Id, category.Id);
	}
}
