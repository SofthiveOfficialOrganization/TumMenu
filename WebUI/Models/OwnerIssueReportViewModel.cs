using System.ComponentModel.DataAnnotations;

namespace WebUI.Models;

public class OwnerIssueReportViewModel
{
    [Required(ErrorMessage = "Hata detayını giriniz.")]
    [StringLength(2000, ErrorMessage = "Hata detayı en fazla 2000 karakter olabilir.")]
    public string Description { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Yapmak istediğiniz işlem en fazla 1000 karakter olabilir.")]
    public string? AttemptedAction { get; set; }

    [Required(ErrorMessage = "Sayfa başlığı alınamadı. Lütfen sayfayı yenileyip tekrar deneyin.")]
    [StringLength(300)]
    public string CurrentPageTitle { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sayfa bağlantısı alınamadı. Lütfen sayfayı yenileyip tekrar deneyin.")]
    [StringLength(1000)]
    public string CurrentPageUrl { get; set; } = string.Empty;

    [StringLength(600)]
    public string? BrowserInfo { get; set; }

    [StringLength(50)]
    public string? Viewport { get; set; }
}
