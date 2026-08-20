using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using System.Linq;
using System.Text.RegularExpressions;

namespace WebUI.ExternalServices
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;
        private readonly string _logFilePath;

        public EmailSender(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
            _logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "email-log.txt");
            EnsureLogDirectoryExists();
        }

        private void EnsureLogDirectoryExists()
        {
            var logDir = Path.GetDirectoryName(_logFilePath);
            if (!Directory.Exists(logDir) && logDir != null)
            {
                Directory.CreateDirectory(logDir);
            }
        }

        private void LogToFile(string message)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var logEntry = $"[{timestamp}] {message}{Environment.NewLine}";
                File.AppendAllText(_logFilePath, logEntry);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write to log file: {ex.Message}");
            }
        }

        private string GetPlainTextLink(string html)
        {
            var match = Regex.Match(html, @"href=""([^""]+)""", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value : "Link bulunamadı";
        }

        private SecureSocketOptions GetSecureSocketOptions()
        {
            if (!_emailSettings.EnableSsl)
            {
                return SecureSocketOptions.None;
            }

            return _emailSettings.Port == 465
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTls;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var requestId = Guid.NewGuid().ToString("N")[..8];
            LogToFile($"=== EMAIL REQUEST START [{requestId}] ===");
            LogToFile($"To: {email}");
            LogToFile($"Subject: {subject}");
            LogToFile($"Body Length: {htmlMessage?.Length ?? 0} characters");
            LogToFile($"SMTP Server: {_emailSettings.SmtpServer}:{_emailSettings.Port}");
            LogToFile($"SSL Enabled: {_emailSettings.EnableSsl}");
            LogToFile($"Username: {_emailSettings.Username}");
            LogToFile($"From: {_emailSettings.FromEmail} ({_emailSettings.FromName})");
            
            try
            {
                Console.WriteLine($"Attempting to send email to {email} via {_emailSettings.SmtpServer}:{_emailSettings.Port}");
                LogToFile("Creating SMTP client...");
                
                using var client = new SmtpClient();
                client.Timeout = 30000; // 30 seconds timeout
                
                LogToFile($"Connecting to {_emailSettings.SmtpServer}:{_emailSettings.Port} with SSL...");
                // Connect to SMTP server
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, GetSecureSocketOptions());
                LogToFile("Connected successfully");
                
                LogToFile($"Authenticating as {_emailSettings.Username}...");
                // Authenticate
                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                LogToFile("Authentication successful");
                
                LogToFile("Creating mail message...");
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
                emailMessage.To.Add(MailboxAddress.Parse(email));
                emailMessage.Subject = subject;
                
                // Create both HTML and plain text versions for better deliverability
                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = htmlMessage;
                
                // Generate plain text version from HTML
                var plainText = System.Text.RegularExpressions.Regex.Replace(htmlMessage ?? string.Empty,
                    @"<[^>]*>", string.Empty).Replace("&nbsp;", " ").Replace("&amp;", "&");
                // Clean up extra whitespace
                while (plainText.Contains("  "))
                    plainText = plainText.Replace("  ", " ");
                plainText = System.Web.HttpUtility.HtmlDecode(plainText);
                
                bodyBuilder.TextBody = $"TumMenu Hesap Onayı\n\nMerhaba,\n\nHesabınızı aktive etmek için aşağıdaki linke tıklayın:\n\n{GetPlainTextLink(htmlMessage ?? string.Empty)}\n\nEğer bu e-postayı talep etmediyseniz lütfen dikkate almayın.\n\nTumMenu Destek Ekibi";
                
                emailMessage.Body = bodyBuilder.ToMessageBody();
                
                // Add anti-spam headers
                emailMessage.Headers.Add("X-Priority", "3");
                emailMessage.Headers.Add("X-Mailer", "TumMenu");
                emailMessage.Headers.Add("Reply-To", _emailSettings.FromEmail);
                
                // Add List-Unsubscribe header for compliance
                emailMessage.Headers.Add("List-Unsubscribe", $"<mailto:{_emailSettings.FromEmail}?subject=Unsubscribe>");
                emailMessage.Headers.Add("List-Unsubscribe-Post", "List-Unsubscribe=One-Click");
                
                LogToFile($"Mail message created with {emailMessage.To.Count} recipient(s)");
                LogToFile($"Message details - From: {emailMessage.From}, To: {emailMessage.To}, Subject: {emailMessage.Subject}");
                LogToFile($"Body preview: {(htmlMessage?.Length > 100 ? htmlMessage.Substring(0, 100) + "..." : htmlMessage)}");

                LogToFile("Attempting to send email...");
                Console.WriteLine("Sending email...");
                
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                try
                {
                    LogToFile("Sending email message...");
                    await client.SendAsync(emailMessage);
                    LogToFile("Email sent successfully, checking server response...");
                    
                    // Get server capabilities and status
                    var capabilities = client.Capabilities;
                    LogToFile($"Server capabilities: {capabilities}");
                    
                    await client.DisconnectAsync(true);
                    stopwatch.Stop();
                    
                    LogToFile($"SMTP command completed in {stopwatch.ElapsedMilliseconds}ms");
                    LogToFile("SMTP client reports success - NOTE: This doesn't guarantee delivery");
                    Console.WriteLine("SMTP command completed successfully");
                    
                    LogToFile($"=== EMAIL REQUEST COMPLETE [{requestId}] ===");
                    return;
                }
                catch (MailKit.Security.AuthenticationException ex)
                {
                    stopwatch.Stop();
                    LogToFile($"Authentication Error: {ex.Message}");
                    LogToFile($"Inner Exception: {ex.InnerException?.Message}");
                    throw new Exception($"SMTP kimlik doğrulaması başarısız oldu: {ex.Message}", ex);
                }
                catch (MailKit.Net.Smtp.SmtpCommandException ex)
                {
                    stopwatch.Stop();
                    LogToFile($"SMTP Command Error: {ex.ErrorCode} - {ex.Message}");
                    LogToFile($"StatusCode: {ex.StatusCode}");
                    throw new Exception($"SMTP Komutu başarısız oldu: {ex.Message}", ex);
                }
                catch (MailKit.Net.Smtp.SmtpProtocolException ex)
                {
                    stopwatch.Stop();
                    LogToFile($"SMTP Protocol Error: {ex.Message}");
                    throw new Exception($"SMTP Protokolü başarısız oldu: {ex.Message}", ex);
                }
                catch (MailKit.Security.SslHandshakeException ex)
                {
                    stopwatch.Stop();
                    LogToFile($"SMTP TLS/SSL Certificate Error: {ex.Message}");
                    LogToFile($"Inner Exception: {ex.InnerException?.Message}");
                    throw new Exception("SMTP sunucusunun TLS/SSL sertifikası doğrulanamadı. Mail sunucusundaki sertifikayı yenileyin veya EmailSettings__SmtpServer değerini geçerli sertifikası olan SMTP ana makine adıyla güncelleyin.", ex);
                }
            }
            catch (Exception ex) when (!(ex is MailKit.Security.AuthenticationException) && 
                                         !(ex is MailKit.Net.Smtp.SmtpCommandException) && 
                                         !(ex is MailKit.Net.Smtp.SmtpProtocolException) &&
                                         !(ex is MailKit.Security.SslHandshakeException))
            {
                LogToFile($"GENERAL ERROR [{requestId}]:");
                LogToFile($"  Message: {ex.Message}");
                LogToFile($"  Inner Exception: {ex.InnerException?.Message}");
                LogToFile($"  Stack Trace: {ex.StackTrace}");
                
                Console.WriteLine($"General Email Error: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                LogToFile($"=== EMAIL REQUEST FAILED [{requestId}] ===");
                throw; // Hatayı yukarı fırlat ki sayfada da görünsün
            }
        }
    }

    public class EmailSettings
    {
        public string SmtpServer { get; set; } = "mail.tummenu.com";
        public int Port { get; set; } = 465;
        public string Username { get; set; } = "destek@tummenu.com";
        public string Password { get; set; } = "zez02102025__";
        public string FromEmail { get; set; } = "destek@tummenu.com";
        public string FromName { get; set; } = "Tüm Menü";
        public bool EnableSsl { get; set; } = true;
    }
}
