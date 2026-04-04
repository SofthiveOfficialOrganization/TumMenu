using Application.Common.Exceptions;
using Application.Companies.Commands;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace WebUI.IntegrationTests.Unit.Companies;

public class CreateCompanyCommandTests
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly Mock<Application.Abstractions.IUserContext> _userContext;

    public CreateCompanyCommandTests()
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

        _userContext = new Mock<Application.Abstractions.IUserContext>();
        _userContext.Setup(x => x.UserId).Returns("test-user-id");
    }

    private EfRepository<T> Repo<T>() where T : class => new(_db);

    [Fact]
    public async Task Handle_ValidCommand_ReturnsCompanyDTO()
    {
        // Arrange
        var owner = new Owner { ApplicationUserId = "test-user-id" };
        await _db.Owners.AddAsync(owner);
        await _db.SaveChangesAsync();

        var handler = new CreateCompanyCommandHandler(Repo<Company>(), Repo<Owner>(), _mapper, _userContext.Object);
        var command = new CreateCompanyCommand { Title = "Test Şirketi", Slug = "test-sirketi" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Test Şirketi");
        result.Slug.Should().Be("test-sirketi");
    }

    [Fact]
    public async Task Handle_DuplicateSlug_ThrowsAlreadyExistsAppException()
    {
        // Arrange
        var owner = new Owner { ApplicationUserId = "test-user-id" };
        await _db.Owners.AddAsync(owner);
        var existing = new Company { Title = "Other", Slug = "test-sirketi", OwnerId = Guid.NewGuid() };
        await _db.Companies.AddAsync(existing);
        await _db.SaveChangesAsync();

        var handler = new CreateCompanyCommandHandler(Repo<Company>(), Repo<Owner>(), _mapper, _userContext.Object);
        var command = new CreateCompanyCommand { Title = "Test Şirketi", Slug = "test-sirketi" };

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AlreadyExistsAppException>();
    }

    [Fact]
    public async Task Handle_WhenCompanyAlreadyExists_ThrowsAlreadyExistsAppException()
    {
        // Arrange
        var owner = new Owner { ApplicationUserId = "test-user-id" };
        await _db.Owners.AddAsync(owner);
        var existing = new Company { Title = "Existing", Slug = "existing", OwnerId = owner.Id };
        await _db.Companies.AddAsync(existing);
        await _db.SaveChangesAsync();

        var handler = new CreateCompanyCommandHandler(Repo<Company>(), Repo<Owner>(), _mapper, _userContext.Object);
        var command = new CreateCompanyCommand { Title = "New Company", Slug = "new-company" };

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AlreadyExistsAppException>();
    }

    [Fact]
    public async Task Handle_WhenOwnerNotFound_ThrowsNotFoundAppException()
    {
        // Arrange — no owner in DB
        var handler = new CreateCompanyCommandHandler(Repo<Company>(), Repo<Owner>(), _mapper, _userContext.Object);
        var command = new CreateCompanyCommand { Title = "Test", Slug = "test" };

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Application.Common.Exceptions.NotFoundAppException>();
    }
}
