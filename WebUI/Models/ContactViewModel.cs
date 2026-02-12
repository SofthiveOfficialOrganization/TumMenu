using System.ComponentModel.DataAnnotations;

namespace WebUI.Models;

public class ContactViewModel
{
    [Required(ErrorMessage = "Adınızı giriniz.")]
    [StringLength(100)]
    [Display(Name = "Adınız")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta adresinizi giriniz.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Konu giriniz.")]
    [StringLength(200)]
    [Display(Name = "Konu")]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mesajınızı giriniz.")]
    [StringLength(2000)]
    [Display(Name = "Mesaj")]
    public string Message { get; set; } = string.Empty;
}
