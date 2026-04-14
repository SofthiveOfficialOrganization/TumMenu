using Domain.Base;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class SystemSetting : BaseEntity
{
    public SystemSettingType Type { get; set; }

    [MaxLength(64)]
    public string Value { get; set; } = "v1";

    [MaxLength(256)]
    public string Description { get; set; } = string.Empty;
}

public enum SystemSettingType
{
    LegalVersionTerms = 1,
    LegalVersionKvkk = 2,
    LegalVersionPrivacy = 3,
    LegalVersionCookie = 4
}
