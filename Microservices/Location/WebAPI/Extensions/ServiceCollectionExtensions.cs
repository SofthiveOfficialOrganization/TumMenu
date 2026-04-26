using System.Reflection;
using Microsoft.EntityFrameworkCore;
using WebAPI.DataAccess.Abstract;
using WebAPI.DataAccess.Concrete;
using WebAPI.DataAccess.Contexts;
using WebAPI.Logging.Providers;
using WebAPI.Logging.Providers.Logger;
using WebAPI.Services.CountryServices;
using WebAPI.Services.DistrictServices;
using WebAPI.Services.GeocodingServices;
using WebAPI.Services.ProvinceServices;

namespace WebAPI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BaseDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("MssqlDBConnection"));
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        services.AddScoped<IProvinceRepository, ProvinceRepository>();
        services.AddScoped<IDistrictRepository, DistrictRepository>();
        services.AddScoped<ICountryRepository, CountryRepository>();
        services.AddScoped<ICountryCurrencyRepository, CountryCurrencyRepository>();
        services.AddScoped<ICountryLanguageRepository, CountryLanguageRepository>();
        services.AddScoped<ICountryTranslationRepository, CountryTranslationRepository>();

        services.AddScoped<IProvinceService, ProvinceService>();
        services.AddScoped<IDistrictService, DistrictService>();
        services.AddScoped<ICountryService, CountryService>();
        services.AddScoped<IGeocodingService, GeocodingService>();

        services.AddSingleton<LoggerServiceBase, FileLogger>();

        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddHttpClient();

        return services;
    }
}
