using Microsoft.AspNetCore.Identity;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace WebUI.Validators
{
    public class TurkishUserValidator : IUserValidator<ApplicationUser>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
        {
            var errors = new List<IdentityError>();

            // Check if email is already taken by another user
            var existingUser = manager.Users.FirstOrDefault(u => 
                u.Email == user.Email && u.Id != user.Id);

            if (existingUser != null)
            {
                errors.Add(new IdentityError
                {
                    Code = "DuplicateEmail",
                    Description = $"'{user.Email}' e-posta adresi zaten alınmış."
                });
            }

            return Task.FromResult(errors.Count == 0 ? 
                IdentityResult.Success : 
                IdentityResult.Failed(errors.ToArray()));
        }
    }
}
