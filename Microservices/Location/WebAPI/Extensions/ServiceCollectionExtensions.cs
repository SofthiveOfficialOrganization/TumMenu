using System.Reflection;
using Microsoft.EntityFrameworkCore;
using WebAPI.DataAccess;
using WebAPI.DataAccess.Abstract;
using WebAPI.DataAccess.Concrete;
using WebAPI.DataAccess.Contexts;
using WebAPI.Logging.Providers;
using WebAPI.Logging.Providers.Logger;
using WebAPI.Models.Concrete;
using WebAPI.Services.AwsServices;
using WebAPI.Services.CountryServices;
using WebAPI.Services.DistrictServices;
using WebAPI.Services.GoogleCloudServices;
using WebAPI.Services.ProvinceServices;

namespace WebAPI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomServices(this IServiceCollection services, IConfiguration configuration)
    {
        #region Add Repositores to DI container
        
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


        #endregion
        
        #region Add Services to DI container
        
        services.AddScoped<IAwsService, AwsService>();
        services.AddScoped<IGoogleCloudService, GoogleCloudService>();
        services.AddScoped<IProvinceService, ProvinceService>();
        services.AddScoped<IDistrictService, DistrictService>();
        services.AddScoped<ICountryService, CountryService>();
        
        //services.AddSingleton<LoggerServiceBase, MsSqlLogger>();
        services.AddSingleton<LoggerServiceBase, FileLogger>();
        #endregion

        #region Add AutoMapper to DI container

        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        #endregion
        
        #region Add HttpClient to DI container

        services.AddHttpClient();

        #endregion
        
        return services;
    }
}
