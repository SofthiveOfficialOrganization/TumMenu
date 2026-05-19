using Microsoft.AspNetCore.Http;
using WebUI.Infrastructure;

namespace WebUI.IntegrationTests.Unit.Infrastructure;

public class AdminRouteDetectorTests
{
	[Theory]
	[InlineData("Admin", "/Admin/Product/Edit", true)]
	[InlineData(null, "/admin/Dashboard", true)]
	[InlineData(null, "/owner/Product/Edit", true)]
	[InlineData(null, "/menu/test", false)]
	[InlineData(null, "/error", false)]
	public void IsAdminRequest_DetectsAdminRoutes(string? area, string path, bool expected)
	{
		var result = AdminRouteDetector.IsAdminRequest(area, path);
		Assert.Equal(expected, result);
	}

	[Fact]
	public void StashOriginalRequest_PersistsAreaAndPath()
	{
		var context = new DefaultHttpContext();
		AdminRouteDetector.StashOriginalRequest(context, "Admin", "/Admin/Product/Edit");

		var (area, path) = AdminRouteDetector.GetStashed(context);

		Assert.Equal("Admin", area);
		Assert.Equal("/Admin/Product/Edit", path);
	}
}
