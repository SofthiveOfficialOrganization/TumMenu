using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


		public virtual ICollection<IdentityUserClaim<string>> Claims { get; set; } = [];
		public virtual ICollection<IdentityUserLogin<string>> Logins { get; set; } = [];
		public virtual ICollection<IdentityUserToken<string>> Tokens { get; set; } = [];
		public virtual ICollection<ApplicationUserRole> UserRoles { get; set; } = [];

	}

	public class ApplicationRole : IdentityRole
	{
		public ApplicationRole()
		{

		}
		public ApplicationRole(string roleName) : base(roleName)
		{

		}
		public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
	}

	public class ApplicationUserRole : IdentityUserRole<string>
	{
		public virtual ApplicationUser User { get; set; }
		public virtual ApplicationRole Role { get; set; }
	}
}
