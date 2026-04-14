using Application.Abstractions;
using Application.SystemSettings.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Application.SystemSettings.Queries;

public record GetLegalVersionSettingsQuery : IRequest<LegalVersionSettingsDTO>;

public class GetLegalVersionSettingsQueryHandler(
    IRepository<SystemSetting> repo,
    IMemoryCache cache
) : IRequestHandler<GetLegalVersionSettingsQuery, LegalVersionSettingsDTO>
{
    private const string CacheKey = "system_settings_legal_versions_v1";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public async Task<LegalVersionSettingsDTO> Handle(GetLegalVersionSettingsQuery request, CancellationToken ct)
    {
        if (cache.TryGetValue(CacheKey, out LegalVersionSettingsDTO? cached) && cached is not null)
        {
            return cached;
        }

        var settings = await repo.Query(tracked: true)
            .Where(s =>
                s.Type == SystemSettingType.LegalVersionTerms ||
                s.Type == SystemSettingType.LegalVersionKvkk ||
                s.Type == SystemSettingType.LegalVersionPrivacy ||
                s.Type == SystemSettingType.LegalVersionCookie)
            .ToListAsync(ct);

        var defaults = GetDefaults();
        var hasChanges = false;
        var newSettings = new List<SystemSetting>();

        foreach (var missing in defaults.Where(d => settings.All(s => s.Type != d.Type)))
        {
            var toCreate = new SystemSetting
            {
                Type = missing.Type,
                Value = missing.Value,
                Description = missing.Description
            };
            settings.Add(toCreate);
            newSettings.Add(toCreate);
            hasChanges = true;
        }

        if (hasChanges)
        {
            foreach (var newSetting in newSettings)
            {
                await repo.AddAsync(newSetting, ct);
            }

            await repo.SaveChangesAsync(ct);
        }

        var dto = new LegalVersionSettingsDTO
        {
            TermsVersion = GetValue(settings, SystemSettingType.LegalVersionTerms, defaults),
            TermsDescription = GetDescription(settings, SystemSettingType.LegalVersionTerms, defaults),
            KvkkVersion = GetValue(settings, SystemSettingType.LegalVersionKvkk, defaults),
            KvkkDescription = GetDescription(settings, SystemSettingType.LegalVersionKvkk, defaults),
            PrivacyVersion = GetValue(settings, SystemSettingType.LegalVersionPrivacy, defaults),
            PrivacyDescription = GetDescription(settings, SystemSettingType.LegalVersionPrivacy, defaults),
            CookieVersion = GetValue(settings, SystemSettingType.LegalVersionCookie, defaults),
            CookieDescription = GetDescription(settings, SystemSettingType.LegalVersionCookie, defaults)
        };

        cache.Set(CacheKey, dto, CacheDuration);
        return dto;
    }

    private static string GetValue(
        IEnumerable<SystemSetting> settings,
        SystemSettingType type,
        IReadOnlyCollection<SystemSetting> defaults)
    {
        return settings.FirstOrDefault(s => s.Type == type)?.Value
            ?? defaults.First(d => d.Type == type).Value;
    }

    private static string GetDescription(
        IEnumerable<SystemSetting> settings,
        SystemSettingType type,
        IReadOnlyCollection<SystemSetting> defaults)
    {
        return settings.FirstOrDefault(s => s.Type == type)?.Description
            ?? defaults.First(d => d.Type == type).Description;
    }

    public static IReadOnlyCollection<SystemSetting> GetDefaults()
    {
        return
        [
            new SystemSetting
            {
                Type = SystemSettingType.LegalVersionTerms,
                Value = "v1",
                Description = "Kullanım koşulları sürümü"
            },
            new SystemSetting
            {
                Type = SystemSettingType.LegalVersionKvkk,
                Value = "v1",
                Description = "KVKK aydınlatma metni sürümü"
            },
            new SystemSetting
            {
                Type = SystemSettingType.LegalVersionPrivacy,
                Value = "v1",
                Description = "Gizlilik politikası sürümü"
            },
            new SystemSetting
            {
                Type = SystemSettingType.LegalVersionCookie,
                Value = "v1",
                Description = "Çerez politikası sürümü"
            }
        ];
    }
}
