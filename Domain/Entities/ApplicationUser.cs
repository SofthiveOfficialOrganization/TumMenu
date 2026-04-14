using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string Name => $"{FirstName} {LastName}";

        [MaxLength(150)]
        public string? FirstName { get; set; }

        [MaxLength(150)]
        public string? LastName { get; set; }

        public long? StatusUpdateDate { get; set; }
        public long CreatedOn { get; set; }
        public long? ModifiedOn { get; set; }
        public int? LockOutCount { get; set; }
        public bool IsLockedOut { get; set; }
        public long LastLogin { get; set; }
        public DateTime? BirthDate { get; set; }

        [MaxLength(64)]
        public string? AcceptedTermsVersion { get; set; }

        [MaxLength(64)]
        public string? AcceptedKvkkVersion { get; set; }

        [MaxLength(64)]
        public string? AcceptedPrivacyVersion { get; set; }

        public DateTime? LegalAcceptedAtUtc { get; set; }

        [MaxLength(64)]
        public string? LegalAcceptedIp { get; set; }

        [MaxLength(512)]
        public string? LegalAcceptedUserAgent { get; set; }

        public Owner? Owner { get; set; }
        public virtual ICollection<IdentityUserClaim<string>> Claims { get; set; } = new List<IdentityUserClaim<string>>();
        public virtual ICollection<IdentityUserLogin<string>> Logins { get; set; } = new List<IdentityUserLogin<string>>();
        public virtual ICollection<IdentityUserToken<string>> Tokens { get; set; } = new List<IdentityUserToken<string>>();
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();
    }

    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole() { }
        public ApplicationRole(string roleName) : base(roleName) { }

        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();
    }

    public class ApplicationUserRole : IdentityUserRole<string>
    {
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ApplicationRole Role { get; set; } = null!;
    }
}
