using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using WebUI.Models.Email;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace WebUI.Services
{
    public interface IEmailTemplateService
    {
        string GenerateRegistrationEmail(RegistrationEmailModel model);
        string GeneratePasswordResetEmail(PasswordResetEmailModel model);
    }

    public class EmailTemplateService : IEmailTemplateService
    {
        public string GenerateRegistrationEmail(RegistrationEmailModel model)
        {
            var html = $@"<!DOCTYPE html>
<html lang=""tr"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>TumMenu - Hesap Onayı</title>
    <style>
        body{{font-family:Arial,sans-serif;background:#f8f9fa;margin:0;padding:20px;line-height:1.5}}
        .container{{max-width:600px;margin:0 auto;background:#fff;border-radius:8px;overflow:hidden;box-shadow:0 2px 10px rgba(0,0,0,0.1)}}
        .header{{background:linear-gradient(135deg,#688745 0%,#495E31 100%);padding:40px 30px;text-align:center}}
        .logo{{font-size:28px;font-weight:900;color:#fff;margin-bottom:8px;letter-spacing:-.02em}}
        .tagline{{color:rgba(255,255,255,.9);font-size:16px;font-weight:500}}
        .body{{background:#fff;padding:50px 40px}}
        .title{{color:#333;font-size:28px;font-weight:700;margin-bottom:20px;text-align:center;line-height:1.2}}
        .message{{color:#555;font-size:16px;line-height:1.6;margin-bottom:30px;text-align:center}}
        .feature{{background:#FAFAF9;border-radius:16px;padding:24px;margin-bottom:20px;border-left:4px solid #688745}}
        .feature-title{{color:#333;font-size:18px;font-weight:600;margin-bottom:8px}}
        .feature-desc{{color:#666;font-size:15px;line-height:1.5}}
        .btn{{display:inline-block;padding:16px 32px;border-radius:12px;text-decoration:none;font-weight:600;font-size:16px;color:#fff;background:linear-gradient(135deg,#688745 0%,#495E31 100%);border:1px solid #688745;box-shadow:0 4px 12px rgba(104,135,69,.25);transition:all .2s ease}}
        .btn:hover{{background:linear-gradient(135deg,#495E31 0%,#3A4A28 100%);transform:translateY(-1px);box-shadow:0 6px 16px rgba(104,135,69,.35)}}
        .footer{{background:#FAFAF9;padding:30px 40px;text-align:center;border-top:1px solid #E5E7EB}}
        .footer-text{{color:#666;font-size:13px;margin-bottom:8px}}
        @media(max-width:600px){{.container{{margin:10px}}.header,.body,.footer{{padding:20px}}}}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <div class=""logo"">TumMenu</div>
            <div class=""tagline"">Restoran Yönetim Sistemi</div>
        </div>
        <div class=""body"">
            <h1 class=""title"">Hoş Geldiniz!</h1>
            <p class=""message"">
                Merhaba <strong>{HtmlEncoder.Default.Encode(model.UserName)}</strong>, TumMenu'ya hoş geldiniz. 
                Hesabınızı aktive etmek için aşağıdaki butona tıklayın.
            </p>
            <div style=""text-align:center;margin:40px 0"">
                <a href=""{HtmlEncoder.Default.Encode(model.AccountActivationUrl)}"" class=""btn"">
                    Hesabımı Aktive Et
                </a>
            </div>
            <div class=""feature"">
                <div class=""feature-title"">📱 Dijital Menü</div>
                <div class=""feature-desc"">QR kod ile menünüze kolayca erişim sağlayın. Müşterileriniz telefonlarıyla menünüzü anında görüntüleyebilir.</div>
            </div>
            <div class=""feature"">
                <div class=""feature-title"">📊 Analitik ve Raporlama</div>
                <div class=""feature-desc"">Restoranınızın performansını gerçek zamanlı olarak takip edin. En çok sipariş edilen ürünleri ve müşteri tercihlerini analiz edin.</div>
            </div>
            <div class=""feature"">
                <div class=""feature-title"">🎨 Kolay Yönetim</div>
                <div class=""feature-desc"">Ürünlerinizi, fiyatlarınızı ve menünüzü tek tıkla güncelleyin. Modern arayüzümüzle yönetim hiç bu kadar kolay olmamıştı.</div>
            </div>
            <p class=""message"" style=""font-size:14px;color:#888"">
                Bu e-postayı talep etmediyseniz lütfen dikkate almayın.
                Hesabınızı aktive ettikten sonra menünüzü oluşturmaya başlayabilirsiniz.
            </p>
        </div>
        <div class=""footer"">
            <p class=""footer-text"">&copy; 2025 TumMenu. Tum haklari saklidir.</p>
            <p class=""footer-text"">Bu e-posta TumMenu tarafindan gonderilmistir. Destek icin: destek@tummenu.com</p>
        </div>
    </div>
</body>
</html>";

            return html;
        }

        public string GeneratePasswordResetEmail(PasswordResetEmailModel model)
        {
            var html = $@"<!DOCTYPE html>
<html lang=""tr"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>TumMenu - Şifre Sıfırlama</title>
</head>
<body style=""font-family:Arial,sans-serif;background:#FAFAF9;margin:0;padding:20px;line-height:1.5"">
    <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width:600px;margin:0 auto;background:#ffffff;border-radius:8px;overflow:hidden;border:1px solid #688745"">
        <tr>
            <td style=""background:#688745;padding:40px 30px;text-align:center"">
                <div style=""font-size:28px;font-weight:900;color:#ffffff;margin-bottom:8px;letter-spacing:-.02em"">TumMenu</div>
                <div style=""color:rgba(255,255,255,.9);font-size:16px;font-weight:500"">Restoran Yönetim Sistemi</div>
            </td>
        </tr>
        <tr>
            <td style=""background:#ffffff;padding:50px 40px"">
                <h1 style=""color:#333;font-size:28px;font-weight:700;margin-bottom:20px;text-align:center;line-height:1.2"">Şifre Sıfırlama İsteği</h1>
                <p style=""color:#666;font-size:16px;line-height:1.6;margin-bottom:30px;text-align:center"">
                    Merhaba <strong>{HtmlEncoder.Default.Encode(model.UserName)}</strong>, 
                    hesabınız için bir şifre sıfırlama talebi aldık.
                </p>
                
                <div style=""background:#fff3cd;border:1px solid #ffeaa7;border-radius:8px;padding:20px;margin:30px 0;text-align:center"">
                    <div style=""color:#856404;font-size:18px;font-weight:600;margin-bottom:8px"">⚠️ Güvenlik Uyarısı</div>
                    <div style=""color:#856404;font-size:14px;line-height:1.5"">
                        Bu işlemi siz yapmadıysanız lütfen bu e-postayı dikkate almayın.
                        Şifreniz sadece aşağıdaki linke tıklayarak değiştirilebilir.
                    </div>
                </div>
                
                <div style=""text-align:center;margin:40px 0"">
                    <a href=""{HtmlEncoder.Default.Encode(model.PasswordResetUrl)}"" 
                       style=""display:inline-block;padding:16px 32px;border-radius:8px;text-decoration:none;font-weight:600;font-size:16px;color:#ffffff;background:#688745;border:1px solid #688745"">
                        Şifremi Sıfırla
                    </a>
                </div>
                
                <div style=""background:#F5E6D3;border-radius:8px;padding:20px;margin:30px 0"">
                    <div style=""color:#333;font-size:16px;font-weight:600;margin-bottom:12px"">🔒 Güvenlik Bilgileri</div>
                    <div style=""color:#666;font-size:14px;line-height:1.5;margin-bottom:8px"">
                        • Bu link {HtmlEncoder.Default.Encode(model.ExpiryHours)} saat geçerlidir
                    </div>
                    <div style=""color:#666;font-size:14px;line-height:1.5"">
                        • Bir kez kullanıldıktan sonra geçersiz olur
                    </div>
                </div>
                
                <p style=""color:#666;font-size:16px;line-height:1.6;margin-bottom:30px;text-align:center"">
                    Eğer şifre sıfırlama talebinde bulunmadıysanız, 
                    hesabınız güvende olabilir. Lütfen hesabınızı kontrol edin.
                </p>
                
                <p style=""font-size:14px;color:#888;text-align:center"">
                    Bu e-postayı talep etmediyseniz lütfen dikkate almayın.
                    Şifrenizi düzenli olarak güncellemeyi unutmayın.
                </p>
            </td>
        </tr>
        <tr>
            <td style=""background:#FAFAF9;padding:30px 40px;text-align:center;border-top:1px solid #688745"">
                <p style=""color:#666;font-size:13px;margin-bottom:8px"">&copy; 2025 TumMenu. Tüm hakları saklıdır.</p>
                <p style=""color:#666;font-size:13px"">Bu e-posta TumMenu tarafından gönderilmiştir. Destek için: destek@tummenu.com</p>
            </td>
        </tr>
    </table>
</body>
</html>";

            return html;
        }
    }
}
