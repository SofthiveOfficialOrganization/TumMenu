using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using CsvHelper;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using MimeKit;

var root = FindRepoRoot();
var archiveExistingOnly = args.Contains("--archive-existing-only", StringComparer.OrdinalIgnoreCase);
var dryRun = args.Contains("--dry-run", StringComparer.OrdinalIgnoreCase);
var positionalArgs = args
    .Where(arg => !string.Equals(arg, "--archive-existing-only", StringComparison.OrdinalIgnoreCase)
        && !string.Equals(arg, "--dry-run", StringComparison.OrdinalIgnoreCase))
    .ToArray();
var expectedArgumentCount = archiveExistingOnly
    ? !dryRun && positionalArgs.Length is >= 1 and <= 2
    : positionalArgs.Length is >= 4 and <= 5;
if (!expectedArgumentCount)
{
    Console.Error.WriteLine(archiveExistingOnly
        ? "Usage: dotnet run -- --archive-existing-only <result-csv> [web.config]"
        : "Usage: dotnet run -- <recipient-csv> <result-csv> <template.md> <category> [web.config]");
    Environment.ExitCode = 2;
    return;
}

var recipientCsv = archiveExistingOnly ? string.Empty : positionalArgs[0];
var resultCsv = archiveExistingOnly ? positionalArgs[0] : positionalArgs[1];
var templatePath = archiveExistingOnly ? string.Empty : positionalArgs[2];
var category = archiveExistingOnly ? string.Empty : positionalArgs[3];
var webConfigArgumentIndex = archiveExistingOnly ? 1 : 4;
var webConfigPath = positionalArgs.Length > webConfigArgumentIndex
    ? positionalArgs[webConfigArgumentIndex]
    : Path.Combine(root, "web.config");

var settings = LoadSettings(webConfigPath);
if (archiveExistingOnly)
{
    await ArchiveExistingSentResultsAsync(settings, resultCsv);
    return;
}

var template = LoadTemplate(templatePath);
var recipients = LoadRecipients(recipientCsv, category);
if (File.Exists(resultCsv))
{
    throw new IOException($"Sonuç dosyası zaten var; üzerine yazmamak için gönderim durduruldu: {resultCsv}");
}

var resultDirectory = Path.GetDirectoryName(Path.GetFullPath(resultCsv));
if (string.IsNullOrWhiteSpace(resultDirectory) || !Directory.Exists(resultDirectory))
{
    throw new DirectoryNotFoundException($"Sonuç klasörü bulunamadı: {resultDirectory}");
}

var results = recipients
    .Select(recipient => new SendResult("", recipient.Organization, recipient.Email, "Queued", ""))
    .ToList();

if (dryRun)
{
    Console.WriteLine($"Kuru çalıştırma — gönderim yapılmadı.");
    Console.WriteLine($"Kategori: {category}");
    Console.WriteLine($"Alıcı sayısı: {recipients.Count}");
    Console.WriteLine($"Konu: {template.Subject}");
    Console.WriteLine($"Sonuç dosyası: {resultCsv} (oluşturulmadı)");
    return;
}

await WriteResultsAsync(resultCsv, results);

foreach (var recipient in recipients)
{
    var sentAt = DateTimeOffset.Now;
    var resultIndex = results.FindIndex(result => string.Equals(result.Email, recipient.Email, StringComparison.OrdinalIgnoreCase));
    results[resultIndex] = new SendResult(
        sentAt.ToString("s", CultureInfo.InvariantCulture),
        recipient.Organization,
        recipient.Email,
        "Sending",
        "");
    await WriteResultsAsync(resultCsv, results);

    var (status, error) = await SendOneAsync(settings, recipient, sentAt, template);
    results[resultIndex] = new SendResult(
        sentAt.ToString("s", CultureInfo.InvariantCulture),
        recipient.Organization,
        recipient.Email,
        status,
        error);
    await WriteResultsAsync(resultCsv, results);

    Console.WriteLine($"{recipient.Organization}: {status}");
    if (!string.IsNullOrWhiteSpace(error))
    {
        Console.WriteLine($"  {error}");
    }

    await Task.Delay(TimeSpan.FromSeconds(2));
}

await WriteResultsAsync(resultCsv, results);

static async Task<(string Status, string Error)> SendOneAsync(
    MailSettings settings,
    Recipient recipient,
    DateTimeOffset sentAt,
    MailTemplate template)
{
    var sentAtText = sentAt.ToString("s", CultureInfo.InvariantCulture);
    var archiveId = CreateArchiveId(recipient.Email, sentAtText);
    MimeMessage message;

    try
    {
        message = CreateMessage(settings, recipient.Email, sentAt, archiveId, template);
        using var client = new SmtpClient { Timeout = 30000 };
        await client.ConnectAsync(settings.SmtpServer, settings.Port, settings.SocketOptions);
        await client.AuthenticateAsync(settings.Username, settings.Password);
        await client.SendAsync(message);
        try
        {
            await client.DisconnectAsync(true);
        }
        catch
        {
            // SMTP already accepted the message; archive it rather than reporting a send failure.
        }
    }
    catch (Exception ex)
    {
        return ("Failed", ex.Message);
    }

    try
    {
        await AppendToSentAsync(settings, message, archiveId, sentAt);
        return ("Sent", "");
    }
    catch (Exception ex)
    {
        return ("SentButNotArchived", $"SMTP kabul etti; IMAP arşivlemesi başarısız: {ex.Message}");
    }
}

static MimeMessage CreateMessage(
    MailSettings settings,
    string to,
    DateTimeOffset sentAt,
    string archiveId,
    MailTemplate template)
{
    var message = new MimeMessage();
    message.Date = sentAt;
    message.From.Add(new MailboxAddress(settings.FromName, settings.FromEmail));
    message.To.Add(MailboxAddress.Parse(to));
    message.Subject = template.Subject;
    message.Headers.Add("X-TumMenu-Campaign-Archive-Id", archiveId);
    message.Headers.Add("Reply-To", settings.FromEmail);
    message.Headers.Add("List-Unsubscribe", $"<mailto:{settings.FromEmail}?subject=RET>");
    message.Body = new TextPart("plain") { Text = template.Body };
    return message;
}

static MailTemplate LoadTemplate(string path)
{
    var content = File.ReadAllText(path);
    const string subjectPrefix = "**Konu:**";
    var subjectLine = File.ReadLines(path)
        .FirstOrDefault(line => line.StartsWith(subjectPrefix, StringComparison.Ordinal));
    if (subjectLine is null)
    {
        throw new InvalidDataException("Şablonda **Konu:** satırı bulunamadı.");
    }

    var subject = subjectLine[subjectPrefix.Length..].Trim();
    var blockStart = content.IndexOf("```text", StringComparison.Ordinal);
    if (string.IsNullOrWhiteSpace(subject) || blockStart < 0)
    {
        throw new InvalidDataException("Şablonda konu veya text e-posta gövdesi eksik.");
    }

    var bodyStart = blockStart + "```text".Length;
    if (content.AsSpan(bodyStart).StartsWith("\r\n")) bodyStart += 2;
    else if (content.AsSpan(bodyStart).StartsWith("\n")) bodyStart++;
    var blockEnd = content.IndexOf("```", bodyStart, StringComparison.Ordinal);
    if (blockEnd < 0)
    {
        throw new InvalidDataException("Şablonun text bloğu kapatılmamış.");
    }

    var body = content[bodyStart..blockEnd].TrimEnd('\r', '\n');
    if (string.IsNullOrWhiteSpace(body))
    {
        throw new InvalidDataException("Şablonun e-posta gövdesi boş.");
    }

    return new MailTemplate(subject, body);
}

static List<Recipient> LoadRecipients(string path, string category)
{
    using var reader = new StreamReader(path);
    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
    if (!csv.Read() || !csv.ReadHeader())
    {
        throw new InvalidDataException("Alıcı CSV dosyasında başlık satırı bulunamadı.");
    }

    var recipients = new List<Recipient>();
    while (csv.Read())
    {
        string Field(string name) => csv.GetField(name)?.Trim() ?? string.Empty;
        var recipient = new Recipient(
            Field("Kategori"),
            Field("Şehir"),
            Field("Kurum"),
            Field("Email"),
            Field("GonderimDurumu"),
            Field("ResmiKaynak"),
            Field("Not"));

        if (!string.Equals(recipient.Category, category, StringComparison.Ordinal)
            || !string.Equals(recipient.SendStatus, "Beklemede", StringComparison.OrdinalIgnoreCase))
        {
            continue;
        }

        if (string.IsNullOrWhiteSpace(recipient.Email)
            || recipient.Email.Any(char.IsWhiteSpace)
            || recipient.Email.IndexOf('@') < 0)
        {
            throw new InvalidDataException($"Geçersiz e-posta adresi: {recipient.Organization}");
        }

        recipients.Add(recipient);
    }

    if (recipients
        .GroupBy(recipient => recipient.Email, StringComparer.OrdinalIgnoreCase)
        .Any(group => group.Count() > 1))
    {
        throw new InvalidDataException("Seçilen kategoride yinelenen e-posta adresi var; gönderim durduruldu.");
    }

    if (recipients.Count == 0)
    {
        throw new InvalidDataException($"'{category}' kategorisinde Beklemede durumunda alıcı bulunamadı.");
    }

    return recipients;
}

static List<SendResult> LoadResults(string path)
{
    if (!File.Exists(path))
    {
        throw new FileNotFoundException("Arşivlenecek gönderim sonuç CSV dosyası bulunamadı.", path);
    }

    using var reader = new StreamReader(path);
    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
    return csv.GetRecords<SendResult>().ToList();
}

static async Task ArchiveExistingSentResultsAsync(MailSettings settings, string resultCsv)
{
    var successfulSends = LoadResults(resultCsv)
        .Where(result => result.Status is "Sent" or "SentButNotArchived")
        .ToArray();

    if (successfulSends.Length == 0)
    {
        Console.WriteLine("Arşivlenecek başarılı gönderim kaydı bulunamadı; SMTP gönderimi yapılmadı.");
        return;
    }

    using var client = new ImapClient { Timeout = 30000 };
    await client.ConnectAsync(settings.ImapServer, settings.ImapPort, settings.ImapSocketOptions);
    await client.AuthenticateAsync(settings.ImapUsername, settings.ImapPassword);
    var sentFolder = await FindSentFolderAsync(client, settings);
    await sentFolder.OpenAsync(FolderAccess.ReadWrite);

    var archiveResults = new List<SendResult>(successfulSends.Length);
    foreach (var result in successfulSends)
    {
        if (!DateTimeOffset.TryParse(
                result.SentAt,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal,
                out var sentAt))
        {
            archiveResults.Add(result with
            {
                Status = "ArchiveFailed",
                Error = "Gönderim zamanı okunamadı; kopya oluşturulmadı."
            });
            continue;
        }

        var archiveId = CreateArchiveId(result.Email, result.SentAt);
        var existing = await sentFolder.SearchAsync(
            SearchQuery.HeaderContains("X-TumMenu-Campaign-Archive-Id", archiveId));
        if (existing.Count > 0)
        {
            archiveResults.Add(result with { Status = "AlreadyArchived", Error = "" });
            continue;
        }

        var message = CreateLegacyMunicipalArchiveMessage(settings, result.Email, sentAt, archiveId);
        await sentFolder.AppendAsync(message, MessageFlags.Seen, sentAt);
        archiveResults.Add(result with { Status = "Archived", Error = "" });
    }

    await client.DisconnectAsync(true);

    var archiveReportPath = CreateArchiveReportPath(resultCsv);
    await WriteResultsAsync(archiveReportPath, archiveResults);
    var archivedCount = archiveResults.Count(result => result.Status == "Archived");
    var alreadyArchivedCount = archiveResults.Count(result => result.Status == "AlreadyArchived");
    var failedCount = archiveResults.Count(result => result.Status == "ArchiveFailed");
    Console.WriteLine($"Tarihçe arşivlendi: yeni={archivedCount}, önceden_arşivli={alreadyArchivedCount}, başarısız={failedCount}");
    Console.WriteLine($"Arşiv raporu: {archiveReportPath}");
}

static MimeMessage CreateLegacyMunicipalArchiveMessage(
    MailSettings settings,
    string to,
    DateTimeOffset sentAt,
    string archiveId)
{
    const string legacySubject = "Belediye lokanta ve kafeleriniz için ücretsiz QR menü desteği";
    const string legacyBody = """
Sayın Yetkili,

Ben Emir Baran Özdemir, TümMenü adına yazıyorum.

TümMenü, restoran, kafe, pastane, belediye lokantası, sosyal tesis, otel restoranı, sahil işletmesi ve benzeri yeme-içme noktalarının QR kod ile kolayca dijital menü oluşturmasını sağlayan yerli bir platformdur. İşletmeler menülerini, kategorilerini, ürünlerini, fiyatlarını, görsellerini, şubelerini ve iletişim bilgilerini tek panelden yönetebilir; vatandaşlar ve ziyaretçiler ise herhangi bir uygulama indirmeden telefondan güncel menüye ulaşabilir.

Belediyenize ait lokanta, kafe, sosyal tesis veya benzeri yeme-içme noktalarında TümMenü QR dijital menü altyapısını sınırsız süreyle tamamen ücretsiz olarak kurmak isteriz. Bu kapsamda belediyenize herhangi bir yazılım geliştirme veya teknik operasyon yükü getirmeden, menülerin dijital ortamda daha kolay yönetilmesini ve vatandaşların güncel bilgilere hızlı ulaşmasını hedefliyoruz.

Turgutlu Belediyesi için de bu sistem kullanıma alınmıştır. Benzer şekilde belediyenize ait işletmelerde de TümMenü'nün değerlendirilmesini isteriz.

TümMenü belediyenize şu konularda katkı sağlayabilir:

- Belediye lokanta, kafe ve sosyal tesislerinde QR kod ile güncel menü sunumu
- Basılı menü ihtiyacının ve menü güncelleme maliyetlerinin azaltılması
- Fiyat, görsel ve ürün bilgilerinin tek panelden hızlı güncellenmesi
- Vatandaşların telefondan kolay, hızlı ve uygulamasız menü erişimi
- Yerel esnafa ve turizm işletmelerine örnek olabilecek dijital dönüşüm modeli
- İlçenin gastronomi, turizm ve yerel işletme görünürlüğüne katkı

Uygun görürseniz konuyla ilgili telefondan iletişime geçerek belediyenize ait lokanta/kafe/sosyal tesisler için ücretsiz QR menü kurulum sürecini aktarabiliriz. Bu e-postaya yanıt vererek de bize ulaşabilirsiniz.

İlgili birim veya yetkili kişi tarafından aranabilirsek memnun oluruz.

Bu tür tanıtım e-postalarını almak istemiyorsanız bu e-postayı "RET" yazarak yanıtlayın. Talebinizi kaydedecek ve adresinize tekrar tanıtım e-postası göndermeyeceğiz.

Saygılarımızla,

Emir Baran Özdemir
TümMenü
+90 543 291 14 53
https://tummenu.com
""";

    var message = new MimeMessage();
    message.Date = sentAt;
    message.From.Add(new MailboxAddress(settings.FromName, settings.FromEmail));
    message.To.Add(MailboxAddress.Parse(to));
    message.Subject = legacySubject;
    message.Headers.Add("X-TumMenu-Campaign-Archive-Id", archiveId);
    message.Body = new TextPart("plain") { Text = legacyBody };
    return message;
}

static string CreateArchiveReportPath(string resultCsv)
{
    var directory = Path.GetDirectoryName(resultCsv) ?? string.Empty;
    var fileName = Path.GetFileNameWithoutExtension(resultCsv);
    var timestamp = DateTimeOffset.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
    var path = Path.Combine(directory, $"{fileName}-arsiv-{timestamp}.csv");
    var sequence = 1;
    while (File.Exists(path))
    {
        path = Path.Combine(directory, $"{fileName}-arsiv-{timestamp}-{sequence++}.csv");
    }

    return path;
}

static string CreateArchiveId(string email, string sentAt)
{
    var value = $"{email.Trim().ToLowerInvariant()}|{sentAt}";
    var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
    return Convert.ToHexString(hash);
}

static async Task AppendToSentAsync(
    MailSettings settings,
    MimeMessage message,
    string archiveId,
    DateTimeOffset internalDate)
{
    using var client = new ImapClient { Timeout = 30000 };
    await client.ConnectAsync(settings.ImapServer, settings.ImapPort, settings.ImapSocketOptions);
    await client.AuthenticateAsync(settings.ImapUsername, settings.ImapPassword);

    var sentFolder = await FindSentFolderAsync(client, settings);
    await sentFolder.OpenAsync(FolderAccess.ReadWrite);

    var existing = await sentFolder.SearchAsync(
        SearchQuery.HeaderContains("X-TumMenu-Campaign-Archive-Id", archiveId));
    if (existing.Count == 0)
    {
        await sentFolder.AppendAsync(message, MessageFlags.Seen, internalDate);
    }

    await client.DisconnectAsync(true);
}

static async Task<IMailFolder> FindSentFolderAsync(ImapClient client, MailSettings settings)
{
    if (!string.IsNullOrWhiteSpace(settings.SentFolder))
    {
        try
        {
            var configured = client.GetFolder(settings.SentFolder);
            if (configured?.Exists == true)
            {
                return configured;
            }
        }
        catch (FolderNotFoundException)
        {
            // Continue with the server's special-use folder and discovered names.
        }
    }

    try
    {
        var special = client.GetFolder(SpecialFolder.Sent);
        if (special?.Exists == true)
        {
            return special;
        }
    }
    catch (FolderNotFoundException)
    {
        // Some providers do not advertise SPECIAL-USE flags.
    }

    var folders = new Dictionary<string, IMailFolder>(StringComparer.OrdinalIgnoreCase);
    if (client.Inbox is not null)
    {
        folders[client.Inbox.FullName] = client.Inbox;
    }

    var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    foreach (var personalNamespace in client.PersonalNamespaces)
    {
        await DiscoverChildrenAsync(client.GetFolder(personalNamespace), folders, visited);
    }

    var sentFolder = folders.Values.FirstOrDefault(folder => folder.Attributes.HasFlag(FolderAttributes.Sent))
        ?? folders.Values.FirstOrDefault(folder =>
            string.Equals(folder.Name, "Sent", StringComparison.OrdinalIgnoreCase)
            || string.Equals(folder.Name, "Sent Items", StringComparison.OrdinalIgnoreCase)
            || string.Equals(folder.Name, "Gönderilenler", StringComparison.OrdinalIgnoreCase));

    return sentFolder ?? throw new FolderNotFoundException("Gönderilenler klasörü bulunamadı.");
}

static async Task DiscoverChildrenAsync(
    IMailFolder parent,
    IDictionary<string, IMailFolder> folders,
    ISet<string> visited)
{
    if (!visited.Add(parent.FullName))
    {
        return;
    }

    // HasChildren is advisory; query LIST so providers with incomplete flags still work.
    var children = await parent.GetSubfoldersAsync(false);
    foreach (var child in children)
    {
        folders.TryAdd(child.FullName, child);
        await DiscoverChildrenAsync(child, folders, visited);
    }
}

static async Task WriteResultsAsync(string path, IEnumerable<SendResult> results)
{
    await using var writer = new StreamWriter(path);
    await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
    await csv.WriteRecordsAsync(results);
}

static MailSettings LoadSettings(string path)
{
    var doc = XDocument.Load(path);
    var values = doc.Descendants("environmentVariable")
        .Where(x => x.Attribute("name") is not null)
        .ToDictionary(x => x.Attribute("name")!.Value, x => x.Attribute("value")?.Value ?? "");

    string Required(string key)
    {
        if (!values.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{key} ayarı web.config içinde bulunamadı.");
        }

        return value;
    }

    var port = int.Parse(Required("EmailSettings__Port"), CultureInfo.InvariantCulture);
    var enableSsl = bool.Parse(Required("EmailSettings__EnableSsl"));
    var socketOptions = !enableSsl
        ? SecureSocketOptions.None
        : port == 465
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTls;

    var imapServer = values.TryGetValue("EmailSettings__ImapServer", out var configuredImapServer)
        && !string.IsNullOrWhiteSpace(configuredImapServer)
            ? configuredImapServer
            : Required("EmailSettings__SmtpServer");
    var imapPort = values.TryGetValue("EmailSettings__ImapPort", out var configuredImapPort)
        && int.TryParse(configuredImapPort, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedImapPort)
            ? parsedImapPort
            : 993;
    var imapSsl = values.TryGetValue("EmailSettings__ImapEnableSsl", out var configuredImapSsl)
        && bool.TryParse(configuredImapSsl, out var parsedImapSsl)
            ? parsedImapSsl
            : true;
    var imapSocketOptions = !imapSsl
        ? SecureSocketOptions.None
        : imapPort == 993
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTls;
    var imapUsername = values.TryGetValue("EmailSettings__ImapUsername", out var configuredImapUsername)
        && !string.IsNullOrWhiteSpace(configuredImapUsername)
            ? configuredImapUsername
            : Required("EmailSettings__Username");
    var imapPassword = values.TryGetValue("EmailSettings__ImapPassword", out var configuredImapPassword)
        && !string.IsNullOrWhiteSpace(configuredImapPassword)
            ? configuredImapPassword
            : Required("EmailSettings__Password");
    values.TryGetValue("EmailSettings__SentFolder", out var sentFolder);

    return new MailSettings(
        Required("EmailSettings__SmtpServer"),
        port,
        Required("EmailSettings__Username"),
        Required("EmailSettings__Password"),
        Required("EmailSettings__FromEmail"),
        "Emir Baran Özdemir TümMenü",
        socketOptions,
        imapServer,
        imapPort,
        imapUsername,
        imapPassword,
        imapSocketOptions,
        sentFolder);
}

static string FindRepoRoot()
{
    var current = new DirectoryInfo(AppContext.BaseDirectory);
    while (current is not null)
    {
        if (File.Exists(Path.Combine(current.FullName, "TumMenu.sln")))
        {
            return current.FullName;
        }

        current = current.Parent;
    }

    throw new InvalidOperationException("Repo kökü bulunamadı.");
}

public sealed record Recipient(
    string Category,
    string City,
    string Organization,
    string Email,
    string SendStatus,
    string SourceUrl,
    string Note);

public sealed record MailTemplate(string Subject, string Body);

public sealed record SendResult(
    string SentAt,
    string Belediye,
    string Email,
    string Status,
    string Error);

public sealed record MailSettings(
    string SmtpServer,
    int Port,
    string Username,
    string Password,
    string FromEmail,
    string FromName,
    SecureSocketOptions SocketOptions,
    string ImapServer,
    int ImapPort,
    string ImapUsername,
    string ImapPassword,
    SecureSocketOptions ImapSocketOptions,
    string? SentFolder);
