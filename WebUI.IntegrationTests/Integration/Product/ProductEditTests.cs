using System.Net;
using Domain.Entities;
using Infrastructure.Persistence;
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
		Assert.Contains("Güncelle", html);
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

	private async Task<(Guid ProductId, Guid CategoryId)> SeedProductAsync()
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
			BasePrice = 100m,
			CategoryId = category.Id,
			SortOrder = 1,
			IsActive = true
		};
		db.Products.Add(product);
		await db.SaveChangesAsync();

		return (product.Id, category.Id);
	}
}
