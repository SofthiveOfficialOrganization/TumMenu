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
    }

    public class EmailTemplateService : IEmailTemplateService
    {
        public string GenerateRegistrationEmail(RegistrationEmailModel model)
        {
            var html = $@"
<!DOCTYPE html>
<html lang=""tr"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <meta name=""format-detection"" content=""telephone=no"">
    <title>TumMenu - Hesap Onayı</title>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{ font-family: Arial, sans-serif; background-color: #f8f9fa; padding: 20px; line-height: 1.5; }}
        .email-container {{ max-width: 600px; margin: 0 auto; background: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1); }}
        .email-header {{ background: #667eea; padding: 30px; text-align: center; }}
        .logo {{ font-size: 24px; font-weight: bold; color: white; margin-bottom: 8px; }}
        .tagline {{ color: rgba(255, 255, 255, 0.9); font-size: 14px; }}
        .email-body {{ background: white; padding: 40px 30px; }}
        .welcome-title {{ color: #333; font-size: 24px; font-weight: 600; margin-bottom: 20px; text-align: center; }}
        .welcome-message {{ color: #555; font-size: 16px; line-height: 1.6; margin-bottom: 30px; text-align: center; }}
        .feature-box {{ background: #f8f9fa; border-radius: 8px; padding: 20px; margin-bottom: 20px; border-left: 4px solid #667eea; }}
        .feature-title {{ color: #333; font-size: 16px; font-weight: 600; margin-bottom: 8px; }}
        .feature-description {{ color: #666; font-size: 14px; line-height: 1.5; }}
        .button-container {{ text-align: center; margin: 30px 0; }}
        .btn {{ display: inline-block; padding: 15px 30px; border-radius: 6px; text-decoration: none; font-weight: 600; font-size: 16px; margin: 10px 5px; }}
        .btn-primary {{ background: #667eea; color: white; }}
        .btn-primary:hover {{ background: #5a6fd8; }}
        .email-footer {{ background: #f8f9fa; padding: 20px; text-align: center; border-top: 1px solid #e9ecef; }}
        .footer-text {{ color: #666; font-size: 12px; margin-bottom: 10px; }}
        .divider {{ height: 1px; background: #e9ecef; margin: 20px 0; }}
        @media (max-width: 600px) {{ .email-container {{ margin: 10px; }} .email-header, .email-body, .email-footer {{ padding: 20px; }} .btn {{ display: block; margin: 10px auto; width: 90%; }} }}
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""email-header"">
            <div class=""logo"">TumMenu</div>
            <div class=""tagline"">Restoran Yönetim Sistemi</div>
        </div>
        
        <div class=""email-body"">
            <h1 class=""welcome-title"">Hoş Geldiniz!</h1>
            <p class=""welcome-message"">
                Merhaba <strong>{HtmlEncoder.Default.Encode(model.UserName)}</strong>, TumMenu'ya hoş geldiniz. 
                Hesabınızı aktive etmek için aşağıdaki butona tıklayın.
            </p>
            
            <div class=""divider""></div>
            
            <div class=""feature-box"">
                <div class=""feature-title"">📱 Dijital Menü</div>
                <div class=""feature-description"">
                    QR kod ile menünüze kolayca erişim sağlayın. Müşterileriniz telefonlarıyla menünüzü anında görüntüleyebilir.
                </div>
            </div>
            
            <div class=""feature-box"">
                <div class=""feature-title"">📊 Analitik ve Raporlama</div>
                <div class=""feature-description"">
                    Restoranınızın performansını gerçek zamanlı olarak takip edin. En çok sipariş edilen ürünleri ve müşteri tercihlerini analiz edin.
                </div>
            </div>
            
            <div class=""feature-box"">
                <div class=""feature-title"">🎨 Kolay Yönetim</div>
                <div class=""feature-description"">
                    Ürünlerinizi, fiyatlarınızı ve menünüzü tek tıkla güncelleyin. Modern arayüzümüzle yönetim hiç bu kadar kolay olmamıştı.
                </div>
            </div>
            
            <div class=""button-container"">
                <a href=""{HtmlEncoder.Default.Encode(model.AccountActivationUrl)}"" class=""btn btn-primary"">
                    Hesabımı Aktive Et
                </a>
            </div>
            
            <p class=""welcome-message"" style=""font-size: 14px; color: #888;"">
                Bu e-postayı talep etmediyseniz lütfen dikkate almayın.
                Hesabınızı aktive ettikten sonra menünüzü oluşturmaya başlayabilirsiniz.
            </p>
        </div>
        
        <div class=""email-footer"">
            <p class=""footer-text"">
                © 2025 TumMenu. Tüm hakları saklıdır.
            </p>
            <p class=""footer-text"">
                Bu e-posta TumMenu tarafından gönderilmiştir. Destek için: destek@tummenu.com
            </p>
        </div>
    </div>
</body>
</html>";

            return html;
        }
    }
}
