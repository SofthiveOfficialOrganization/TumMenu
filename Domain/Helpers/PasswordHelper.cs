using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Helpers
{
	public static class PasswordHelper
	{
		private static readonly string Pepper = "YOUR_APP_SECRET_PEPPER";

		public static void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
		{
			using var hmac = new HMACSHA512();
			salt = hmac.Key;
			var combined = password + Pepper;
			hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(combined));
		}

		public static bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
		{
			using var hmac = new HMACSHA512(storedSalt);
			var combined = password + Pepper;
			var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(combined));
			return storedHash.SequenceEqual(computedHash);
		}
	}

}
