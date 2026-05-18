using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Moq;
using WebUI.Areas.Admin.Controllers;

namespace WebUI.IntegrationTests.Unit.Admin;

public class PlaygroundControllerTests
{
    [Fact]
    public void Index_WhenSimulationEnabled_SetsViewDataFlag()
    {
        var controller = CreateController(Environments.Production, simulationEnabled: true);

        var result = Assert.IsType<ViewResult>(controller.Index());

        Assert.True(result.ViewData["CanThrowServerError"] as bool?);
    }

    [Fact]
    public void ThrowServerError_WhenSimulationDisabled_ReturnsNotFound()
    {
        var controller = CreateController(Environments.Production, simulationEnabled: false);

        var result = controller.ThrowServerError();

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void ThrowServerError_WhenSimulationEnabled_Throws()
    {
        var controller = CreateController(Environments.Production, simulationEnabled: true);

        var ex = Assert.Throws<InvalidOperationException>(() => controller.ThrowServerError());
        Assert.Equal("Intentional admin 500 simulation.", ex.Message);
    }

    [Fact]
    public void ThrowServerError_InDevelopment_ThrowsWithoutConfigFlag()
    {
        var controller = CreateController(Environments.Development, simulationEnabled: false);

        Assert.Throws<InvalidOperationException>(() => controller.ThrowServerError());
    }

    private static PlaygroundController CreateController(string environmentName, bool simulationEnabled)
    {
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(x => x.EnvironmentName).Returns(environmentName);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Diagnostics:EnableAdmin500Simulation"] = simulationEnabled.ToString()
            })
            .Build();

        return new PlaygroundController(environment.Object, configuration);
    }
}
