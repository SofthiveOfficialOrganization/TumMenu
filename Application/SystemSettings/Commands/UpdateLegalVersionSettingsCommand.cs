using Application.Abstractions;
using Application.SystemSettings.DTOs;
using Application.SystemSettings.Queries;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel.DataAnnotations;

namespace Application.SystemSettings.Commands;

public class UpdateLegalVersionSettingsCommand : IRequest<LegalVersionSettingsDTO>, ITransactionalRequest
{
    [Required]
    [StringLength(64)]
    public string TermsVersion { get; set; } = "v1";

    [StringLength(256)]
    public string TermsDescription { get; set; } = string.Empty;

    [Required]
    [StringLength(64)]
    public string KvkkVersion { get; set; } = "v1";

    [StringLength(256)]
    public string KvkkDescription { get; set; } = string.Empty;

    [Required]
    [StringLength(64)]
    public string PrivacyVersion { get; set; } = "v1";

    [StringLength(256)]
    public string PrivacyDescription { get; set; } = string.Empty;

    [Required]
    [StringLength(64)]
    public string CookieVersion { get; set; } = "v1";

    [StringLength(256)]
    public string CookieDescription { get; set; } = string.Empty;
}

public class UpdateLegalVersionSettingsCommandValidator : AbstractValidator<UpdateLegalVersionSettingsCommand>
{
    public UpdateLegalVersionSettingsCommandValidator()
    {
        RuleFor(x => x.TermsVersion)
            .NotEmpty().WithMessage("Kullanım koşulları sürümü boş olamaz.")
            .MaximumLength(64).WithMessage("Kullanım koşulları sürümü en fazla 64 karakter olabilir.");

        RuleFor(x => x.KvkkVersion)
            .NotEmpty().WithMessage("KVKK sürümü boş olamaz.")
            .MaximumLength(64).WithMessage("KVKK sürümü en fazla 64 karakter olabilir.");

        RuleFor(x => x.PrivacyVersion)
            .NotEmpty().WithMessage("Gizlilik politikası sürümü boş olamaz.")
            .MaximumLength(64).WithMessage("Gizlilik politikası sürümü en fazla 64 karakter olabilir.");

        RuleFor(x => x.CookieVersion)
            .NotEmpty().WithMessage("Çerez politikası sürümü boş olamaz.")
            .MaximumLength(64).WithMessage("Çerez politikası sürümü en fazla 64 karakter olabilir.");

        RuleFor(x => x.TermsDescription)
            .MaximumLength(256).WithMessage("Kullanım koşulları açıklaması en fazla 256 karakter olabilir.");

        RuleFor(x => x.KvkkDescription)
            .MaximumLength(256).WithMessage("KVKK açıklaması en fazla 256 karakter olabilir.");

        RuleFor(x => x.PrivacyDescription)
            .MaximumLength(256).WithMessage("Gizlilik politikası açıklaması en fazla 256 karakter olabilir.");

        RuleFor(x => x.CookieDescription)
            .MaximumLength(256).WithMessage("Çerez politikası açıklaması en fazla 256 karakter olabilir.");
    }
}

public class UpdateLegalVersionSettingsCommandHandler(
    IRepository<SystemSetting> repo,
    IMemoryCache cache
) : IRequestHandler<UpdateLegalVersionSettingsCommand, LegalVersionSettingsDTO>
{
    private const string CacheKey = "system_settings_legal_versions_v1";

    public async Task<LegalVersionSettingsDTO> Handle(UpdateLegalVersionSettingsCommand req, CancellationToken ct)
    {
        var settings = await repo.Query(tracked: true)
            .Where(s =>
                s.Type == SystemSettingType.LegalVersionTerms ||
                s.Type == SystemSettingType.LegalVersionKvkk ||
                s.Type == SystemSettingType.LegalVersionPrivacy ||
                s.Type == SystemSettingType.LegalVersionCookie)
            .ToListAsync(ct);

        var defaults = GetLegalVersionSettingsQueryHandler.GetDefaults();
        var newSettings = new List<SystemSetting>();

        Upsert(
            settings,
            newSettings,
            defaults,
            SystemSettingType.LegalVersionTerms,
            req.TermsVersion.Trim(),
            string.IsNullOrWhiteSpace(req.TermsDescription)
                ? defaults.First(d => d.Type == SystemSettingType.LegalVersionTerms).Description
                : req.TermsDescription.Trim());

        Upsert(
            settings,
            newSettings,
            defaults,
            SystemSettingType.LegalVersionKvkk,
            req.KvkkVersion.Trim(),
            string.IsNullOrWhiteSpace(req.KvkkDescription)
                ? defaults.First(d => d.Type == SystemSettingType.LegalVersionKvkk).Description
                : req.KvkkDescription.Trim());

        Upsert(
            settings,
            newSettings,
            defaults,
            SystemSettingType.LegalVersionPrivacy,
            req.PrivacyVersion.Trim(),
            string.IsNullOrWhiteSpace(req.PrivacyDescription)
                ? defaults.First(d => d.Type == SystemSettingType.LegalVersionPrivacy).Description
                : req.PrivacyDescription.Trim());

        Upsert(
            settings,
            newSettings,
            defaults,
            SystemSettingType.LegalVersionCookie,
            req.CookieVersion.Trim(),
            string.IsNullOrWhiteSpace(req.CookieDescription)
                ? defaults.First(d => d.Type == SystemSettingType.LegalVersionCookie).Description
                : req.CookieDescription.Trim());

        foreach (var setting in newSettings)
        {
            await repo.AddAsync(setting, ct);
        }

        cache.Remove(CacheKey);

        return new LegalVersionSettingsDTO
        {
            TermsVersion = req.TermsVersion.Trim(),
            TermsDescription = string.IsNullOrWhiteSpace(req.TermsDescription)
                ? defaults.First(d => d.Type == SystemSettingType.LegalVersionTerms).Description
                : req.TermsDescription.Trim(),
            KvkkVersion = req.KvkkVersion.Trim(),
            KvkkDescription = string.IsNullOrWhiteSpace(req.KvkkDescription)
                ? defaults.First(d => d.Type == SystemSettingType.LegalVersionKvkk).Description
                : req.KvkkDescription.Trim(),
            PrivacyVersion = req.PrivacyVersion.Trim(),
            PrivacyDescription = string.IsNullOrWhiteSpace(req.PrivacyDescription)
                ? defaults.First(d => d.Type == SystemSettingType.LegalVersionPrivacy).Description
                : req.PrivacyDescription.Trim(),
            CookieVersion = req.CookieVersion.Trim(),
            CookieDescription = string.IsNullOrWhiteSpace(req.CookieDescription)
                ? defaults.First(d => d.Type == SystemSettingType.LegalVersionCookie).Description
                : req.CookieDescription.Trim()
        };
    }

    private static void Upsert(
        IEnumerable<SystemSetting> settings,
        ICollection<SystemSetting> newSettings,
        IEnumerable<SystemSetting> defaults,
        SystemSettingType type,
        string value,
        string description)
    {
        var current = settings.FirstOrDefault(s => s.Type == type);
        if (current is null)
        {
            var defaultDescription = defaults.First(d => d.Type == type).Description;
            newSettings.Add(new SystemSetting
            {
                Type = type,
                Value = value,
                Description = string.IsNullOrWhiteSpace(description) ? defaultDescription : description
            });
            return;
        }

        current.Value = value;
        current.Description = description;
    }
}
