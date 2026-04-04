using Application.QRs.Commands;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace WebUI.IntegrationTests.Unit.QRs;

public class GenerateQRCommandTests
{
    private readonly ApplicationDbContext _db;

    public GenerateQRCommandTests()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(opts);
    }

    private EfRepository<T> Repo<T>() where T : class => new(_db);

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNonEmptyGuid()
    {
        var handler = new GenerateQRCodeCommandHandler(Repo<QRCode>());
        var command = new GenerateQRCodeCommand { StoreId = Guid.NewGuid() };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_SetsPublicKeyAndStoreId()
    {
        var storeId = Guid.NewGuid();
        var handler = new GenerateQRCodeCommandHandler(Repo<QRCode>());
        var command = new GenerateQRCodeCommand { StoreId = storeId };

        var result = await handler.Handle(command, CancellationToken.None);

        var qr = await _db.Set<QRCode>().FindAsync(result);
        qr.Should().NotBeNull();
        qr!.PublicKey.Should().NotBeNullOrEmpty();
        qr.StoreId.Should().Be(storeId);
    }
}
