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

public class DeleteCompanyCommandTests
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;

    public DeleteCompanyCommandTests()
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
    public async Task Handle_ValidDelete_SoftDeletesCompany()
    {
        // Arrange
        var company = new Company { Title = "To Delete", Slug = "to-delete" };
        await _db.Companies.AddAsync(company);
        await _db.SaveChangesAsync();

        var handler = new DeleteCompanyCommandHandler(Repo<Company>(), Repo<Store>(), Repo<Menu>(), _mapper);
        var command = new DeleteCompanyCommand { Id = company.Id };

        // Act
        await handler.Handle(command, CancellationToken.None);
        await _db.SaveChangesAsync();

        // Assert — SoftDelete calls Remove, so the entity should no longer be in the DB
        var found = await _db.Companies.FindAsync(company.Id);
        found.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ValidDelete_AlsoDeletesAssociatedStores()
    {
        // Arrange
        var company = new Company { Title = "Company", Slug = "company" };
        await _db.Companies.AddAsync(company);
        await _db.SaveChangesAsync();

        var store = new Store { CompanyId = company.Id, Title = "Store", Slug = "store", PhoneNumber = "5551234567" };
        await _db.Stores.AddAsync(store);
        await _db.SaveChangesAsync();

        var handler = new DeleteCompanyCommandHandler(Repo<Company>(), Repo<Store>(), Repo<Menu>(), _mapper);
        var command = new DeleteCompanyCommand { Id = company.Id };

        // Act
        await handler.Handle(command, CancellationToken.None);
        await _db.SaveChangesAsync();

        // Assert
        var foundStore = await _db.Stores.FindAsync(store.Id);
        foundStore.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenCompanyNotFound_ThrowsNotFoundAppException()
    {
        // Arrange — no company in DB
        var handler = new DeleteCompanyCommandHandler(Repo<Company>(), Repo<Store>(), Repo<Menu>(), _mapper);
        var command = new DeleteCompanyCommand { Id = Guid.NewGuid() };

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundAppException>();
    }
}
