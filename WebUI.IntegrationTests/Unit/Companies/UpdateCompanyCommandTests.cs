using Application.Common.Exceptions;
using Application.Companies.Commands;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace WebUI.IntegrationTests.Unit.Companies;

public class UpdateCompanyCommandTests
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;

    public UpdateCompanyCommandTests()
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
    }

    private EfRepository<T> Repo<T>() where T : class => new(_db);

    [Fact]
    public async Task Handle_ValidUpdate_PersistsChanges()
    {
        // Arrange
        var company = new Company { Title = "Old Title", Slug = "old-slug" };
        await _db.Companies.AddAsync(company);
        await _db.SaveChangesAsync();

        _db.ChangeTracker.Clear();
        var handler = new UpdateCompanyCommandHandler(Repo<Company>(), _mapper);
        var command = new UpdateCompanyCommand { Id = company.Id, Title = "New Title", Slug = "new-slug" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("New Title");
        result.Slug.Should().Be("new-slug");
    }

    [Fact]
    public async Task Handle_DuplicateSlug_ThrowsAlreadyExistsAppException()
    {
        // Arrange
        var company1 = new Company { Title = "Company 1", Slug = "company-one" };
        var company2 = new Company { Title = "Company 2", Slug = "company-two" };
        await _db.Companies.AddRangeAsync(company1, company2);
        await _db.SaveChangesAsync();

        var handler = new UpdateCompanyCommandHandler(Repo<Company>(), _mapper);
        // Try to update company2 to use company1's slug
        var command = new UpdateCompanyCommand { Id = company2.Id, Title = "Company 2", Slug = "company-one" };

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AlreadyExistsAppException>();
    }

    [Fact]
    public async Task Handle_WhenCompanyNotFound_ThrowsNotFoundAppException()
    {
        // Arrange — no company in DB
        var handler = new UpdateCompanyCommandHandler(Repo<Company>(), _mapper);
        var command = new UpdateCompanyCommand { Id = Guid.NewGuid(), Title = "Test", Slug = "test" };

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundAppException>();
    }
}
