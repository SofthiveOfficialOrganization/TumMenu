using Application.Products.Commands;
using Application.Products.DTOs;
using WebUI.Areas.Admin.Helpers;

namespace WebUI.IntegrationTests.Unit.Areas.Admin;

public class ProductEditViewModelBuilderTests
{
	[Fact]
	public void Merge_OverwritesScalarsAndPrices_PreservesMedias()
	{
		var productId = Guid.NewGuid();
		var existing = new ProductDTO
		{
			Id = productId,
			Title = "Old",
			BasePrice = 10m,
			Medias = [new Application.Medias.DTOs.MediaDTO { Id = Guid.NewGuid() }],
			Prices = [new ProductPriceDTO { Size = "Small", Price = 10m }]
		};

		var command = new UpdateProductCommand
		{
			Id = productId,
			Title = "New",
			BasePrice = 170m,
			CategoryId = Guid.NewGuid(),
			SortOrder = 1,
			IsActive = true,
			Prices =
			[
				new ProductPriceInputDTO { Size = "Menu", Price = 320m }
			]
		};

		var merged = ProductEditViewModelBuilder.Merge(command, existing);

		Assert.Equal("New", merged.Title);
		Assert.Equal(170m, merged.BasePrice);
		Assert.Single(merged.Prices);
		Assert.Equal("Menu", merged.Prices[0].Size);
		Assert.Equal(320m, merged.Prices[0].Price);
		Assert.Single(merged.Medias);
	}
}
