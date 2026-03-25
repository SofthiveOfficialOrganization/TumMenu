namespace WebUI.Models.Email
{
    public class PasswordResetEmailModel
    {
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string PasswordResetUrl { get; set; } = string.Empty;
        public string ExpiryHours { get; set; } = "24";
    }
}
