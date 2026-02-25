using System.Reflection;
using Microsoft.EntityFrameworkCore;
using WebAPI.Models.Concrete;

namespace WebAPI.DataAccess.Contexts;

public class BaseDbContext(
    DbContextOptions dbContextOptions, 
    IConfiguration configuration
    ) : DbContext(dbContextOptions)
{
    protected IConfiguration Configuration { get; set; } = configuration;
    
    public DbSet<Province> Provinces { get; set; }
    public DbSet<District> Districts { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<CountryTranslation> Translations { get; set; }
    public DbSet<CountryLanguage> Languages { get; set; }
    public DbSet<CountryCurrency> Currencies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
