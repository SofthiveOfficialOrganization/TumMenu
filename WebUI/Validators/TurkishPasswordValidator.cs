using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using System.Globalization;
using Domain.Entities;

namespace WebUI.Validators
{
    public class TurkishPasswordValidator : IPasswordValidator<ApplicationUser>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user, string? password)
        {
            var errors = new List<IdentityError>();

            if (string.IsNullOrEmpty(password))
            {
                errors.Add(new IdentityError
                {
                    Code = "PasswordRequired",
                    Description = "Şifre gereklidir."
                });
                return Task.FromResult(IdentityResult.Failed(errors.ToArray()));
            }

            // Check password length
            if (password.Length < 6)
            {
                errors.Add(new IdentityError
                {
                    Code = "PasswordTooShort",
                    Description = "Şifre en az 6 karakter uzunluğunda olmalıdır."
                });
            }

            if (password.Length > 100)
            {
                errors.Add(new IdentityError
                {
                    Code = "PasswordTooLong",
                    Description = "Şifre en fazla 100 karakter uzunluğunda olmalıdır."
                });
            }

            // Check for non-alphanumeric characters
            if (!password.Any(c => !char.IsLetterOrDigit(c)))
            {
                errors.Add(new IdentityError
                {
                    Code = "PasswordRequiresNonAlphanumeric",
                    Description = "Şifre en az bir sembol (@, !, #, vb...) içermelidir."
                });
            }

            // Check for uppercase letters
            if (!password.Any(c => char.IsUpper(c)))
            {
                errors.Add(new IdentityError
                {
                    Code = "PasswordRequiresUpper",
                    Description = "Şifre en az bir büyük harf ('A'-'Z') içermelidir."
                });
            }

            // Check for lowercase letters
            if (!password.Any(c => char.IsLower(c)))
            {
                errors.Add(new IdentityError
                {
                    Code = "PasswordRequiresLower",
                    Description = "Şifre en az bir küçük harf ('a'-'z') içermelidir."
                });
            }

            // Check for digits
            if (!password.Any(c => char.IsDigit(c)))
            {
                errors.Add(new IdentityError
                {
                    Code = "PasswordRequiresDigit",
                    Description = "Şifre en az bir rakam ('0'-'9') içermelidir."
                });
            }

            return Task.FromResult(errors.Count == 0 ? 
                IdentityResult.Success : 
                IdentityResult.Failed(errors.ToArray()));
        }
    }
}
