namespace Application.SystemSettings.DTOs;

public class LegalVersionSettingsDTO
{
    public string TermsVersion { get; set; } = "v1";
    public string TermsDescription { get; set; } = "Kullanım koşulları sürümü";

    public string KvkkVersion { get; set; } = "v1";
    public string KvkkDescription { get; set; } = "KVKK aydınlatma metni sürümü";

    public string PrivacyVersion { get; set; } = "v1";
    public string PrivacyDescription { get; set; } = "Gizlilik politikası sürümü";

    public string CookieVersion { get; set; } = "v1";
    public string CookieDescription { get; set; } = "Çerez politikası sürümü";
}
