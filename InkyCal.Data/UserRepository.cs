using System.Threading.Tasks;
using InkyCal.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InkyCal.Data
{
	public static class UserRepository
	{

		public static async Task<User> GetUser(this IdentityUser identityUser)
		{
			if (identityUser is null)
				return null;

			return await GetUser(identityUser.Id);
		}

		public static async Task<User> GetUser(this string identityUserId)
		{
			using var c = new ApplicationDbContext();
			var result = await c.Set<User>()
								.SingleOrDefaultAsync(x => x.IdentityUserId == identityUserId);

			if (result is null)
			{
				result = new User() { IdentityUserId = identityUserId };
				c.Add(result);
				await c.SaveChangesAsync();
			}

			return result;
		}
	}
}
