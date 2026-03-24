namespace WebUI.Models.Email
{
    public class RegistrationEmailModel
    {
        public string UserName { get; set; } = string.Empty;
        public string ConfirmationUrl { get; set; } = string.Empty;
        public string AccountActivationUrl { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
    }
}
