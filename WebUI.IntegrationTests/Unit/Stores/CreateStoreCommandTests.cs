using Application.Common.Exceptions;
using Application.Stores.Commands;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace WebUI.IntegrationTests.Unit.Stores;

public class CreateStoreCommandTests
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly Mock<IMediator> _mediator;

    public CreateStoreCommandTests()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(opts);

        var config = new TypeAdapterConfig();
        config.Scan(typeof(Application.Companies.Commands.CreateCompanyCommand).Assembly);
        _mapper = new ServiceMapper(
            new ServiceCollection()
                .AddSingleton(config)
                .AddScoped<IMapper, ServiceMapper>()
                .BuildServiceProvider(),
            config);

        _mediator = new Mock<IMediator>();
        _mediator
            .Setup(m => m.Send(It.IsAny<Application.QRs.Commands.GenerateQRCodeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());
    }

    private EfRepository<T> Repo<T>() where T : class => new(_db);

    [Fact]
    public async Task Handle_ValidCommand_StoreIsPersistedAndLinkedToCompany()
    {
        // Arrange
        var company = new Company { Title = "Test Co", Slug = "test-co" };
        await _db.Companies.AddAsync(company);
        await _db.SaveChangesAsync();

        var handler = new CreateStoreCommandHandler(Repo<Store>(), _mapper, Repo<Company>(), _mediator.Object);
        var command = new CreateStoreCommand
        {
            Title = "Test Store",
            Slug = "test-store",
            PhoneNumber = "05001234567",
            CompanyId = company.Id
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        await _db.SaveChangesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Test Store");

        var savedCount = await _db.Stores.IgnoreQueryFilters().CountAsync();
        savedCount.Should().Be(1);
        var saved = await _db.Stores.IgnoreQueryFilters().FirstAsync();
        saved.CompanyId.Should().Be(company.Id);
    }

    [Fact]
    public async Task Handle_CompanyNotFound_ThrowsUnprocessableAppException()
    {
        // Arrange — no company in DB
        var handler = new CreateStoreCommandHandler(Repo<Store>(), _mapper, Repo<Company>(), _mediator.Object);
        var command = new CreateStoreCommand
        {
            Title = "Test Store",
            Slug = "test-store",
            PhoneNumber = "05001234567",
            CompanyId = Guid.NewGuid()
        };

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnprocessableAppException>();
    }
}
