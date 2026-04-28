using Microsoft.EntityFrameworkCore;
using WebAPI.DataAccess.Contexts;
using WebAPI.Exceptions;
using WebAPI.Services.CountryServices;
using WebAPI.Services.ProvinceServices;

namespace WebAPI.Extensions;

public static class MigrationExtensions
{
    public static async Task MigrateDatabaseAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BaseDbContext>();
        await db.Database.MigrateAsync();
    }

    public static async Task SeedDataAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var provinceService = scope.ServiceProvider.GetRequiredService<IProvinceService>();
        var countryService = scope.ServiceProvider.GetRequiredService<ICountryService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            await provinceService.FetchTurkeyData();
            logger.LogInformation("İl ve ilçe verileri başarıyla yüklendi.");
        }
        catch (BusinessException)
        {
            logger.LogInformation("İl ve ilçe verileri zaten mevcut, seed atlandı.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "İl ve ilçe verileri yüklenirken hata oluştu.");
        }

        try
        {
            await countryService.FetchCountriesData();
            logger.LogInformation("Ülke verileri başarıyla yüklendi.");
        }
        catch (BusinessException)
        {
            logger.LogInformation("Ülke verileri zaten mevcut, seed atlandı.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ülke verileri yüklenirken hata oluştu.");
        }
    }
}
