namespace WebUI.ExternalServices;

public sealed class EmailSettings
{
    public string SmtpServer { get; set; } = "mail.tummenu.com";
    public int Port { get; set; } = 465;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = "destek@tummenu.com";
    public string FromName { get; set; } = "Tüm Menü";
    public bool EnableSsl { get; set; } = true;

    public string ImapServer { get; set; } = "mail.tummenu.com";
    public int ImapPort { get; set; } = 993;
    public string? ImapUsername { get; set; }
    public string? ImapPassword { get; set; }
    public bool ImapEnableSsl { get; set; } = true;
    public string? SentFolder { get; set; }

    public int ConnectionTimeoutSeconds { get; set; } = 30;
    public long MaxAttachmentBytes { get; set; } = 25 * 1024 * 1024;

    public string EffectiveImapUsername =>
        string.IsNullOrWhiteSpace(ImapUsername) ? Username : ImapUsername;

    public string EffectiveImapPassword =>
        string.IsNullOrWhiteSpace(ImapPassword) ? Password : ImapPassword;
}
